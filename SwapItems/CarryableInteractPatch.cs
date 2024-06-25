using CG.Client.Player.Interactions;
using CG.Client.Ship.Interactions;
using CG.Game.Player;
using CG.Objects;
using CG.Ship.Hull;
using Gameplay.Carryables;
using HarmonyLib;
using System;
using UnityEngine;

namespace SwapItems
{
    [HarmonyPatch(typeof(CarryableInteract))]
    internal class CarryableInteractPatch
    {
        private static AbstractCarryableObject item1;
        private static AbstractCarryableObject item2;
        private static CarryablesSocket socket;
        private static LocalPlayer player;
        private static bool running = false;

        [HarmonyPrefix]
        [HarmonyPatch("StartCarryableInteraction", new Type[] { typeof(bool) })]
        static bool StartCarryableInteraction(LocalPlayer ___player, bool ___isFetching, bool ___lockInteraction)
        {
            if (running) return false;
            player = ___player;
            if (___isFetching || ___lockInteraction || player.IsBusy || player.Payload == null) return true;
            SocketInteractable socketInteractable = player.RaycastHandler.Current as SocketInteractable;
            socket = socketInteractable?.SocketActor?.Socket;
            if (socket?.Payload == null || !socket.IsInput || !socket.IsOutput ||
                socket.CurrentState != SocketState.Open || !socket.DoesAccept(player.Payload)) return true;

            running = true;

            item1 = player.Payload;
            item2 = socket.Payload;

            socket.CarryableHandler.TryEjectCarryable(Vector3.up); //Remove item2 from socket
            VoidManager.Events.Instance.LateUpdate += PlaceItem;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("PickupGrabable")]
        static bool PickupGrabable(LocalPlayer ___player, bool ___isFetching, bool ___lockInteraction)
        {
            if (running) return false;
            player = ___player;
            if (___lockInteraction || ___isFetching || player.Payload == null) return true;
            AbstractInteractable abstractInteractable = ___player.RaycastHandler.Current;
            GrabableObject grab = abstractInteractable as GrabableObject;
            if (grab == null) return true;

            running = true;

            item1 = player.Payload;
            item2 = grab.Carryable;
            player.CarryableHandler.TryEjectCarryable(Vector3.down);
            VoidManager.Events.Instance.LateUpdate += GrabItem;
            return false;
        }

        private static void PlaceItem(object sender, EventArgs e)
        {
            if (socket.Payload == null)
            {
                VoidManager.Events.Instance.LateUpdate -= PlaceItem;
                socket.CarryableHandler.TryInsertCarryable(item1); //Place item1 in socket
                VoidManager.Events.Instance.LateUpdate += GrabItem;
            }
        }

        private static void GrabItem(object sender, EventArgs e)
        {
            if (player.Carrier.Payload == null)
            {
                VoidManager.Events.Instance.LateUpdate -= GrabItem;
                player.Carrier.TryInsertCarryable(item2).Then(FixPosition); //Pick up item2 and position it in the hand
                running = false;
            }
        }

        private static void FixPosition()
        {
            GrabableObject grab = item2.GetComponent<GrabableObject>();
            Vector3 holdAngle = (grab.UseOverrideHoldingAngle ? grab.OverrideHoldingAngle : Vector3.zero);
            Transform subContainer = player.Carrier.SubContainer;
            Vector3 b = subContainer.position + subContainer.right * grab.CarryingOffset.x + subContainer.up * grab.CarryingOffset.y + subContainer.forward * grab.CarryingOffset.z;
            grab.transform.position = b;
            grab.transform.rotation = subContainer.rotation * Quaternion.Euler(holdAngle);
        }
    }
}

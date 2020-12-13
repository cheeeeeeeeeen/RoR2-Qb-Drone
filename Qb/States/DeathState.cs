using Chen.GradiusMod.Drones;
using RoR2;

namespace Chen.Qb.States
{
    internal class DeathState : DroneDeathState
    {
        protected override InteractableSpawnCard GetInteractableSpawnCard()
        {
            return QbDrone.instance.iSpawnCard;
        }
    }
}
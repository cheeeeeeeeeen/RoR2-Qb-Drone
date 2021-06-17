using Chen.GradiusMod.Drones;
using RoR2;

namespace Chen.Qb.States
{
    internal class DeathState : DroneDeathState
    {
        protected override bool SpawnInteractable { get; set; } = QbDrone.instance.canBeRepurchased;

        protected override InteractableSpawnCard GetInteractableSpawnCard => QbDrone.instance.iSpawnCard;
    }
}
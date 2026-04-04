
namespace TC2.Base.Components
{
	public static partial class LandMine
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			Armed = 1 << 0,
			Faction = 1 << 1,

			Ready = 1 << 2,
			No_Disarm = 1 << 3,
		}

		public static readonly Sound.Handle sound_arm_default = "item.adjust.03";
		public static readonly Sound.Handle sound_trigger_default = "item.adjust.02";

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Save.NewLine]
			[Save.Force] public required LandMine.Flags flags;

			[Save.NewLine]
			public byte frame_default = 0;
			public byte frame_armed = 1;
			public Sound.Handle sound_arm = LandMine.sound_arm_default;
			public Sound.Handle sound_trigger = LandMine.sound_trigger_default;
			
			[Save.NewLine]
			[Save.Force] public required Mass mass_min = 10.00f.kg();
			//public Mass mass_max;
		}

#if CLIENT
[Shitcode]
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.10f)]
		public static void UpdateSprite(
		[Source.Owned] ref LandMine.Data landmine, [Source.Owned] ref Animated.Renderer.Data renderer,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			renderer.sprite.frame.y = landmine.flags.HasAny(LandMine.Flags.Armed) ? landmine.frame_armed : landmine.frame_default;
		}
#endif

#if SERVER
		[ISystem.Event<Interactable.InteractEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnInteract(ISystem.Info info, Entity ent_landmine, ref Region.Data region, ref XorRandom random, [Source.Owned] ref Interactable.InteractEvent data,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Interactable.Data interactable, [Source.Owned] ref LandMine.Data landmine)
		{
			if (landmine.flags.HasAny(LandMine.Flags.No_Disarm)) return;

			landmine.flags.ToggleFlag(LandMine.Flags.Armed);
			landmine.flags.RemoveFlag(LandMine.Flags.Ready);
			landmine.Sync(ent_landmine);

			Sound.Play(region: ref region, sound: landmine.sound_arm, world_position: transform.position, volume: 0.60f, pitch: 1.00f, priority: 0.60f, size: 0.90f);
		}

		[Shitcode]
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region,
		[Source.Owned] ref LandMine.Data landmine, [Source.Owned] ref Explosive.Data explosive,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction)
		{
			if (landmine.flags.HasAny(LandMine.Flags.Armed) && explosive.flags.HasNone(Explosive.Flags.Primed) && body.HasArbiters())
			{
				if (landmine.flags.HasNone(LandMine.Flags.Ready) && body.IsSleeping())
				{
					landmine.flags.AddFlag(LandMine.Flags.Ready);
					return;
				}

				foreach (var arbiter in body.GetArbiters())
				{
					if (arbiter.GetState() == Body.Arbiter.State.Begin)
					{
						var layer = arbiter.GetLayer();
						if (layer.HasAny(Physics.Layer.Dynamic) && layer.HasAny(Physics.Layer.Item | Physics.Layer.Creature))
						{
							var mass = arbiter.GetBodyMassActual();
							if (mass < landmine.mass_min) continue;
							//if (landmine.mass_max.m_value.IsNil() ) continue;

							if (landmine.flags.HasAny(LandMine.Flags.Faction) && faction.id != 0 && arbiter.GetFaction() == faction.id) continue;

							Sound.Play(region: ref region, sound: landmine.sound_trigger, world_position: transform.position, volume: 0.70f, pitch: 0.80f, priority: 0.60f);

							//landmine.flags.RemoveFlag(LandMine.Flags.Ready);
							explosive.flags.AddFlag(Explosive.Flags.Primed);
							entity.Delete();

							//arbiter.SetIgnored();

							break;
						}
					}
				}
			}
		}
#endif
	}
}

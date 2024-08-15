
namespace TC2.Base.Components
{
	public static partial class LandMine
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Armed = 1 << 0,
			Faction = 1 << 1
		}

		public static readonly Sound.Handle sound_arm_default = "item.adjust.03";
		public static readonly Sound.Handle sound_trigger_default = "item.adjust.02";

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public Sound.Handle sound_arm = LandMine.sound_arm_default;
			public Sound.Handle sound_trigger = LandMine.sound_trigger_default;

			public uint frame_default = 0;
			public uint frame_armed = 1;

			public LandMine.Flags flags;

			public Data()
			{

			}
		}

#if CLIENT
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.10f)]
		public static void UpdateSprite(ISystem.Info info, Entity entity,
		[Source.Owned] ref LandMine.Data landmine, [Source.Owned] ref Animated.Renderer.Data renderer,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			renderer.sprite.frame.Y = landmine.flags.HasAny(LandMine.Flags.Armed) ? landmine.frame_armed : landmine.frame_default;
		}
#endif

#if SERVER
		[ISystem.Event<Interactable.InteractEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnInteract(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random, [Source.Owned] ref Interactable.InteractEvent data,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Interactable.Data interactable, [Source.Owned] ref Body.Data body, [Source.Owned] ref LandMine.Data landmine)
		{
			Sound.Play(ref region, landmine.sound_arm, transform.position, volume: 0.60f, pitch: 1.00f, priority: 0.60f, size: 0.90f);
			landmine.flags.SetFlag(LandMine.Flags.Armed, !landmine.flags.HasAny(LandMine.Flags.Armed));

			//App.WriteLine("arm");

			landmine.Sync(entity);
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region,
		[Source.Owned] ref LandMine.Data landmine, [Source.Owned] ref Explosive.Data explosive,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction)
		{
			if (body.HasArbiters() && landmine.flags.HasAny(LandMine.Flags.Armed) && !explosive.flags.HasAny(Explosive.Flags.Primed))
			{
				foreach (var arbiter in body.GetArbiters())
				{
					if (arbiter.GetState() == Body.Arbiter.State.Begin)
					{
						var layer = arbiter.GetLayer();
						if (layer.HasAll(Physics.Layer.Dynamic) && layer.HasAny(Physics.Layer.Item | Physics.Layer.Creature))
						{
							if (landmine.flags.HasAny(LandMine.Flags.Faction) && faction.id != 0 && arbiter.GetFaction() == faction.id) continue;

							Sound.Play(ref region, landmine.sound_trigger, transform.position, volume: 0.70f, pitch: 0.80f, priority: 0.60f);

							explosive.flags.SetFlag(Explosive.Flags.Primed, true);
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

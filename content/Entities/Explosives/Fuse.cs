
namespace TC2.Base.Components
{
	public static partial class Fuse
	{
		public static Texture.Handle tex_spark = "spark";
		public static Sound.Handle sound_extinguish_default = "fuse_extinguish";

		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Sparkle = 1u << 0
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Duration", description: "Burn time.", format: "{0:0.00} s", comparison: Statistics.Comparison.None, priority: Statistics.Priority.High)]
			public float time;
			[Statistics.Info("Failure Chance", description: "Chance to stop burning when lit.", format: "{0:P2}", comparison: Statistics.Comparison.Lower, priority: Statistics.Priority.Low)]
			public float failure_chance;

			public Sound.Handle sound;
			public Sound.Handle sound_extinguish = Fuse.sound_extinguish_default;

			public Vector2 sparkle_offset;

			public Fuse.Flags flags;
			[Net.Ignore] public float failure_time;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("lit", false, Source.Modifier.Owned)]
		public static void OnUpdate(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region,
		[Source.Owned] ref Fuse.Data fuse, [Source.Owned] ref Explosive.Data explosive,
		[Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform, [Source.Owned] in Body.Data body, [Source.Parent, Optional] in Player.Data player)
		{
			if (control.keyboard.GetKeyDown(Keyboard.Key.Spacebar))
			{
				entity.SetTag("lit", true);
				explosive.flags |= Explosive.Flags.Primed;
				explosive.ent_owner = body.GetParent();

				Notification.Push(in player, "Lit a fuse.", Color32BGRA.Yellow, 5.00f);
				Sound.Play(ref region, fuse.sound, transform.position, priority: 0.65f);

				if (fuse.failure_chance > Maths.epsilon && random.NextBool(fuse.failure_chance))
				{
					fuse.failure_time = fuse.time * random.NextFloatRange(0.30f, 0.70f);
				}

				explosive.Sync(entity, true);
				fuse.Sync(entity, true);
			}
		}

		[ISystem.Event<EssenceNode.FailureEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnFailure(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, ref EssenceNode.FailureEvent data, [Source.Owned] ref Fuse.Data fuse)
		{
			fuse.failure_time = fuse.time;
			fuse.failure_chance += random.NextFloatRange(0.10f, 0.80f);
			fuse.failure_chance = fuse.failure_chance.Clamp01();
			fuse.Sync(entity);
		}
#endif

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("lit", true, Source.Modifier.Owned)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, [Source.Owned] ref Fuse.Data fuse, [Source.Owned] in Transform.Data transform)
		{
			fuse.time -= App.fixed_update_interval_s;

#if SERVER
			if (fuse.failure_chance > Maths.epsilon && fuse.time > 0.50f && fuse.time < fuse.failure_time)
			{
				entity.RemoveTag("lit");
				fuse.failure_time = 0.00f;

				Sound.Play(ref region, fuse.sound_extinguish, transform.position, priority: 0.65f);

				fuse.Sync(entity, true);
			}
#endif

#if CLIENT
			if (fuse.flags.HasAll(Fuse.Flags.Sparkle))
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_spark,
					lifetime = random.NextFloatRange(0.05f, 0.15f),
					pos = transform.LocalToWorld(fuse.sparkle_offset),
					frame_count = 3,
					frame_count_total = 3,
					fps = 30,
					rotation = random.NextFloat(MathF.PI),
					vel = random.NextUnitVector2Range(2, 6),
					angular_velocity = random.NextFloat(10),
					force = new Vector2(0, -30.00f),
				});
			}
#endif

#if SERVER
			if (fuse.time <= 0.00f)
			{
				entity.Delete();
			}
#endif
		}

#if CLIENT
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("lit", true, Source.Modifier.Owned)]
		public static void OnUpdateSoundLit([Source.Owned] in Fuse.Data fuse, [Source.Owned, Pair.Of<Fuse.Data>] ref Sound.Emitter sound_emitter)
		{
			sound_emitter.pitch_mult = 1.00f + ((2.00f - MathF.Min(2.00f, fuse.time)) * 0.50f);
			sound_emitter.volume_mult = 1.00f;
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("lit", true, Source.Modifier.Owned)]
		public static void OnUpdateLightLit(ref Region.Data region, ref XorRandom random, [Source.Owned] in Fuse.Data fuse, [Source.Owned, Pair.Of<Fuse.Data>] ref Light.Data light)
		{
			if (fuse.flags.HasAll(Fuse.Flags.Sparkle))
			{
				light.intensity = random.NextFloatRange(0.50f, 1.00f);
			}
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("lit", false, Source.Modifier.Owned)]
		public static void OnUpdateSoundUnlit([Source.Owned] in Fuse.Data fuse, [Source.Owned, Pair.Of<Fuse.Data>] ref Sound.Emitter sound_emitter)
		{
			sound_emitter.volume_mult = 0.00f;
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("lit", false, Source.Modifier.Owned)]
		public static void OnUpdateLightUnlit([Source.Owned] in Fuse.Data fuse, [Source.Owned, Pair.Of<Fuse.Data>] ref Light.Data light)
		{
			light.intensity = 0.00f;
		}
#endif
	}
}

namespace TC2.Base.Components
{
	public static partial class Deteriorator
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Use_Misc = 1 << 15
			}

			[Net.Ignore, Save.Ignore] public float t_next_update;
			public Deteriorator.Data.Flags flags;
			private ushort unused_00;

			[Editor.Picker.Position(true, true)] public Vec2f offset;
			[Editor.Picker.Box(true)] public AABB rect;

		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_deteriorator,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Deteriorator.Data deteriorator)
		{

		}

		[ISystem.Event<EssenceNode.FailureEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnEssenceFailureEvent(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_deteriorator, ref EssenceNode.FailureEvent ev,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Deteriorator.Data deteriorator, [Source.Owned, Pair.Component<Deteriorator.Data>] ref Essence.Emitter.Data essence_emitter)
		{
			App.WriteLine("essence failure event", color: App.Color.Magenta);
		}

		[ISystem.PostUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Essence(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_deteriorator,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Deteriorator.Data deteriorator, [Source.Owned, Pair.Component<Deteriorator.Data>] ref Essence.Emitter.Data essence_emitter)
		{
			if (essence_emitter.flags.HasAny(Essence.Emitter.Flags.Pulsed))
			{
#if CLIENT
				var h_essence = essence_emitter.h_essence; // new IEssence.Handle("motion");
				ref var essence_data = ref h_essence.GetData();
				if (essence_data.IsNotNull())
				{
					var pos = transform.LocalToWorld(essence_emitter.offset);
					var dir = transform.LocalToWorldDirection(essence_emitter.direction);

					var intensity = 1.00f;
					var color_a = ColorBGRA.Lerp(essence_data.color_emit, ColorBGRA.White, 0.50f);
					var color_b = essence_data.color_emit.WithColorMult(1.20f).WithAlphaMult(0.00f);

					//App.WriteLine("essence");

					//Sound.Play(region: ref region, sound: essence_emitter.h_sound_emit, world_position: pos, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
					Sound.Play(region: ref region, h_soundmix: essence_emitter.h_soundmix_test, random: ref random, pos: pos, size: 1.00f, dist_mult: 0.50f); //, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);

					for (var i = 0; i < 3; i++)
					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = Light.tex_light_fire_00,
							lifetime = random.NextFloatExtra(0.30f, 0.20f),
							pos = pos, // + dir,
									   //vel = (dir * 30.00f), // + random.NextUnitVector2Extra(0.50f, 4.00f),
							vel = (dir * random.NextFloatExtra(30.00f, 5.00f)), // + random.NextUnitVector2Extra(0.50f, 4.00f),
							drag = random.NextFloatExtra(0.10f, 0.15f),
							frame_count = 1,
							frame_count_total = 1,
							frame_offset = 0,
							scale = random.NextFloatExtra(1.20f, 0.80f),
							stretch = new Vector2(1.00f, random.NextFloatExtra(0.40f, 0.20f)),
							face_dir_ratio = 1.00f,
							growth = random.NextFloatExtra(8.00f, 8.00f),
							//rotation = random.NextFloat(3.00f),
							angular_velocity = random.NextFloat(5.00f),
							//force = new(random.NextFloat(4), 0.00f),
							color_a = color_a,
							color_b = color_b,
							glow = 10.00f * intensity
						});
					}
					

					Shake.Emit(region: ref region, world_position: pos, trauma: 0.30f, max: 0.30f, radius: 18.00f);

					//Particle.Spawn(ref region, new Particle.Data()
					//{
					//	texture = Light.tex_light_circle_04,
					//	lifetime = 0.30f,
					//	pos = pos - dir,
					//	vel = dir * 30.00f,
					//	drag = 0.15f,
					//	frame_count = 1,
					//	frame_count_total = 1,
					//	frame_offset = 0,
					//	scale = 1.20f,
					//	stretch = new Vector2(1.00f, 0.80f),
					//	face_dir_ratio = 1.00f,
					//	growth = 40.00f,
					//	force = new(random.NextFloat(25), 0.00f),
					//	color_a = color_a,
					//	color_b = color_b,
					//	glow = 20.00f * intensity
					//});
				}
#endif
			}
		}

		[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnPreUpdate(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_deteriorator,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Deteriorator.Data deteriorator,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}

#if CLIENT
		public struct DeterioratorGUI: IGUICommand
		{
			public Entity ent_deteriorator;

			public Transform.Data transform;
			public Crafter.Data crafter;
			public Crafter.State crafter_state;
			public Deteriorator.Data deteriorator;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Deteriorator"u8, this.ent_deteriorator, size: new(48 * 3, 96 * 1)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: GUI.Rm))
						{

						}
					}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region, order: 50)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Deteriorator.Data deteriorator)
		{
			if (interactable.IsActive())
			{
				var gui = new DeterioratorGUI()
				{
					ent_deteriorator = entity,

					transform = transform,
					crafter = crafter,
					crafter_state = crafter_state,
					deteriorator = deteriorator
				};
				gui.Submit();
			}
		}
#endif
	}
}

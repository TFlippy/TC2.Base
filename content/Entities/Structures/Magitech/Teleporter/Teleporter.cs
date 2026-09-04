
namespace TC2.Base.Components
{
	public static partial class Teleporter
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,


		}

		[Flags]
		public enum EffectFlags: ushort
		{
			None = 0,

			No_Particles = 1 << 0,
			No_Sound = 1 << 1,
			No_Shake = 1 << 2,
			No_Shockwave = 1 << 3,
		}

		// WIP
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Save.Force, Editor.Picker.Position(relative: true)] public required Vec2f offset;
			[Save.Force, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Up;

			[Editor.Slider.Clamped(min: 0.00f, max: 16.00f, snap: 0.001f)]
			[Save.Force] public required float radius;
			[Save.Force] public required float power;

			[Save.Force] public required Distance distance_max;
			[Save.Force] public required Mass mass_max;
			[Save.Force] public required Volume volume_max;

			[Save.Force] public Teleporter.Flags flags;
			[Save.Force] public required ISoundMix.Handle h_soundmix_teleport;
			public uint reserved_01;
		}

		//[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		//public partial struct Effect(): IComponent
		//{
		//	[Asset.Ignore] public int current_progress;

		//	public uint unused_00;
		//	public float unused_01;
		//	public float unused_02;
		//}


		[IEvent.Data]
		public partial struct TeleportEvent(): IEvent
		{
			public Vec2f pos;
			public Entity ent_target;

			public float radius;
			public float unused_00;
			public float unused_01;
			public float unused_02;
		}

		public struct DEV_TeleportRPC: Net.IRPC<Teleporter.Data>
		{
			public FixedArray4<Shipment.Item2> items;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Teleporter.Data data)
			{
				//Assert.IsDevMode();
				//Assert.IsAdmin(ref rpc.connection);

				var ev = new Teleporter.TeleportEvent
				{
					pos = rpc.record.GetPosition(),
					radius = data.radius,
					ent_target = rpc.entity
				};
				ev.TriggerDeferred(rpc.entity, sync: true);

				//rpc.entity.Delete();
			}
#endif
		}

		[ISystem.Event<Teleporter.TeleportEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnTeleport(ISystem.Info info, ref Region.Data region, Entity entity, ref XorRandom random,
		[Source.Owned] ref Teleporter.TeleportEvent ev,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Teleporter.Data teleporter)
		{
#if CLIENT
			Teleporter.EmitEffect(ref region, random: ref random, pos: transform.position, radius: ev.radius, intensity: 1.00f, effect_flags: EffectFlags.None);
#endif
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Teleporter.Data teleporter,
		[Source.Owned] in Control.Data control)
		{

		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_B(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
		[Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Teleporter.Data teleporter)
		{

		}

		//[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnUpdate_C(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
		//[Source.Owned] ref Transform.Data transform,
		//[Source.Owned] ref Essence.Teleporter.Data teleporter,
		//[Source.Owned, Pair.Component<Essence.Teleporter.Data>] ref Essence.Charge.Data charge)
		//{

		//}

#if CLIENT
		[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRender_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Teleporter.Data teleporter)
		{

		}
#endif

#if CLIENT
		public static void EmitEffect(ref Region.Data region, ref XorRandom random,
		Vec2f pos, float radius, float intensity, Teleporter.EffectFlags effect_flags = Teleporter.EffectFlags.None)
		{
			App.WriteLine("teleport effect");

			var h_prefab = new Prefab.Handle("teleporter.00");
			var h_soundmix_teleport = new ISoundMix.Handle("teleport.00");

			Sound.Play(ref region, h_soundmix_teleport, ref random, pos: pos, volume: 1.50f);
			Sound.Play(ref region, h_soundmix_teleport, ref random, pos: pos, pitch: 0.90f);
			Sound.Play(ref region, h_soundmix_teleport, ref random, pos: pos, pitch: 0.70f);
			Sound.Play(ref region, h_soundmix_teleport, ref random, pos: pos, pitch: 1.50f);

			var power = intensity;
			var max_radius = radius;

			Shake.Emit(region: ref region, world_position: pos, trauma: 0.90f, max: 1.00f, radius: radius * 20);

			{
				var sprite = h_prefab.GetIcon();

				var num = 30;
				for (var i = 0; i < num; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = sprite.texture,
						lifetime = random.NextFloatExtra(0.04f, 4.50f),
						pos = pos, // + random.NextUnitVector2Extra(0.01f, 0.10f),
						vel = random.NextUnitVector2Extra(0.10f, 0.50f), // random.NextUnitVector2Range(max_radius * 0.12f, max_radius * 0.70f) * 8.00f * i,
																		 //fps = random.NextByteRange(15, 20),
						frame_count = 0,
						frame_count_total = 1,
						frame_offset = (byte)sprite.frame.x, // random.NextByteRange(0, 64),
															 //scale = random.NextFloatExtra(1.00f, 0.10f) * 4,
						scale = random.NextFloatExtra(1.05f, 0.1f),
						rotation = random.NextFloat(0.01f),
						//angular_velocity = (i * 0.01f),
						//angular_velocity = random.NextFloat(0.10f),
						//growth = -random.NextFloatExtra(3.00f, 1.00f),
						//drag = random.NextFloatExtra(0.02f, 0.03f),
						//color_a = ColorBGRA.ARGB(1.50f, 1.10f, 1.10f, 1.10f),
						growth = random.NextFloatExtra(0.15f, -0.25f),
						drag = random.NextFloatExtra(0.04f, 0.04f),
						//color_a = ColorBGRA.ARGB(random.NextFloatExtra(0.50f, 1.00f), 1.10f, 1.10f, 1.10f),
						color_a = ColorBGRA.ARGB(random.NextFloatExtra(0.05f, 0.10f), -random.NextFloatExtra(0.00f, 4.80f), -random.NextFloatExtra(4.00f, 4.10f), random.NextFloatExtra(4.00f, 0.80f)),
						color_b = ColorBGRA.ARGB(0.00f, random.NextFloatExtra(4.00f, -14.00f), -random.NextFloatExtra(4.00f, 10.00f), -random.NextFloatExtra(4.00f, 40.00f)),
						//glow = 1.00f

						stretch = new Vector2(random.NextFloatExtra(0.80f, 0.30f), random.NextFloatExtra(0.90f, 0.20f)),
						face_dir_ratio = random.NextFloatExtra(0.15f, 0.10f),
					});
				}
			}

			{
				var smoke_count = 12;
				for (var i = 0; i < smoke_count; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = Essence.texture_smoke,
						lifetime = random.NextFloatExtra(0.30f, 0.20f),
						pos = pos + random.NextVector2(0.40f),
						vel = random.NextUnitVector2Extra(i * 0.10f, radius) * 14, // random.NextUnitVector2Range(max_radius * 0.12f, max_radius * 0.70f) * 8.00f * i,
						force = random.NextUnitVector2Range(20.00f, 40.00f),
						fps = random.NextByteRange(15, 20),
						frame_count = 64,
						frame_count_total = 64,
						frame_offset = random.NextByteRange(0, 64),
						scale = random.NextFloatExtra(0.40f, 0.30f),
						//rotation = random.NextFloat(10.00f),
						angular_velocity = random.NextFloat(5.00f),
						growth = random.NextFloatExtra(1.00f, 5.00f),
						drag = random.NextFloatExtra(0.02f, 0.03f),
						color_a = ColorBGRA.ARGB(0.50f, 0.90f, 0.90f, 0.90f).WithAlphaMult(random.NextFloatRange(0.60f, 1.00f)),
						color_b = new Color32BGRA(0, 240, 240, 240),
						stretch = new Vector2(random.NextFloatExtra(0.80f, 0.30f), random.NextFloatExtra(0.90f, 0.20f)),
						face_dir_ratio = random.NextFloatExtra(0.75f, 0.10f),
					});
				}
			}

			{
				var smoke_count = 7;
				for (var i = 0; i < smoke_count; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = Essence.texture_smoke,
						lifetime = random.NextFloatExtra(0.20f, 0.40f),
						pos = pos,
						vel = random.NextUnitVector2Extra(i * 0.10f, radius) * 14, // random.NextUnitVector2Range(max_radius * 0.12f, max_radius * 0.70f) * 8.00f * i,
																				   //force = random.NextUnitVector2Range(20.00f, 40.00f),
						fps = random.NextByteRange(15, 20),
						frame_count = 64,
						frame_count_total = 64,
						frame_offset = random.NextByteRange(0, 64),
						scale = random.NextFloatExtra(0.40f, 0.10f) * 2,
						rotation = random.NextFloat(10.00f),
						angular_velocity = random.NextFloat(15.00f),
						growth = -random.NextFloatExtra(3.00f, 1.00f),
						drag = random.NextFloatExtra(0.02f, 0.03f),
						color_a = ColorBGRA.ARGB(0.60f, 0.90f, 0.90f, 0.90f).WithAlphaMult(random.NextFloatRange(0.60f, 1.00f)),
						color_b = new Color32BGRA(0, 240, 240, 240),
						//stretch = new Vector2(random.NextFloatExtra(0.80f, 0.30f), random.NextFloatExtra(0.90f, 0.20f)),
						//face_dir_ratio = random.NextFloatExtra(0.75f, 0.10f),
					});
				}
			}
		}

		public struct TeleporterGUI: IGUICommand
		{
			public Entity ent_teleporter;
			public Transform.Data transform;
			public Teleporter.Data teleporter;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Teleporter"u8, this.ent_teleporter))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						ref var region = ref this.ent_teleporter.GetRegion();

						using (GUI.Group.New(size: GUI.Rm))
						{
							//GUI.TextShaded("Derpo"u8);

							if (GUI.DrawButton("DEV: Teleport"u8, size: new(128, 40)))
							{
								//EmitEffect(region: ref region, random: ref region.GetRandom(), pos: this.transform.position, radius: 1.00f, intensity: 1.00f);
								var rpc = new Teleporter.DEV_TeleportRPC
								{

								};
								rpc.Send(this.ent_teleporter);
							}
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] in Teleporter.Data teleporter)
		{
			if (interactable.IsActive())
			{
				var gui = new TeleporterGUI()
				{
					ent_teleporter = entity,
					transform = transform,
					teleporter = teleporter,
				};
				gui.Submit();
			}
		}
#endif
	}
}

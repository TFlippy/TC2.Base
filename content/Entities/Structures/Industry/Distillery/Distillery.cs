namespace TC2.Base.Components
{
	public static partial class Distillery
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,
			}

			[Editor.Picker.Position(relative: true)] public Vec2f smoke_offset;

			public Distillery.Data.Flags flags;

			[Save.Ignore, Net.Ignore] public float t_next_smoke;
			[Save.Ignore, Net.Ignore] public float t_next_edit;
		}

		public struct ConfigureRPC: Net.IRPC<Distillery.Data>
		{
#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Distillery.Data data)
			{

			}
#endif
		}

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Distillery.Data distillery,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}


#if CLIENT
		public struct DistilleryGUI: IGUICommand
		{
			public Entity ent_distillery;

			public Distillery.Data distillery;
			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Distillery"u8, this.ent_distillery, size: new(48 * 3, 48 * 2), show_misc: true))
				{
					this.StoreCurrentWindowTypeID(order: -50);
					if (window.show)
					{
						using (var group = GUI.Group.New(size: GUI.Rm))
						{
							group.DrawBackground(GUI.tex_frame);
						}
					}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity ent_distillery, [Source.Owned] in Interactable.Data interactable,
		[Source.Owned] in Distillery.Data distillery, [Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state)
		{
			if (interactable.IsActive())
			{
				var gui = new DistilleryGUI()
				{
					ent_distillery = ent_distillery,

					distillery = distillery,
					crafter = crafter,
					crafter_state = crafter_state
				};
				gui.Submit();
			}
		}
#endif

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSound(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Distillery.Data distillery, 
		[Source.Owned, Pair.Component<Distillery.Data>] ref Sound.Emitter sound_emitter)
		{
			//var axle_speed = MathF.Abs(axle_state.angular_velocity);

			//sound_emitter.volume = Maths.Clamp(axle_speed * 0.50f, 0.00f, 0.50f);
			//sound_emitter.pitch = Maths.Clamp(axle_speed * 0.80f, 0.50f, 1.00f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Distillery.Data distillery)
		{
			//var axle_speed = MathF.Abs(axle_state.angular_velocity);
			//if (axle_speed > 1.00f && info.WorldTime >= state.t_next_smoke)
			//{
			//	state.t_next_smoke = info.WorldTime + 0.50f;

			//	Particle.Spawn(ref region, new Particle.Data()
			//	{
			//		texture = texture_smoke,
			//		lifetime = random.NextFloatRange(8.00f, 10.00f),
			//		pos = transform.LocalToWorldNoRotation(distillery.smoke_offset) + random.NextVector2(0.20f),
			//		vel = new Vector2(0.70f, -1.10f) * 1.50f,
			//		force = new Vector2(0.14f, 0.20f) * random.NextFloatRange(0.90f, 1.10f),
			//		fps = (byte)random.NextFloatRange(8, 10),
			//		frame_count = 64,
			//		frame_count_total = 64,
			//		frame_offset = (byte)random.NextFloatRange(0, 64),
			//		scale = random.NextFloatRange(0.20f, 0.40f),
			//		rotation = random.NextFloat(10.00f),
			//		angular_velocity = random.NextFloat(0.50f),
			//		growth = 0.30f,
			//		color_a = new Color32BGRA(180, 197, 217, 160),
			//		color_b = new Color32BGRA(0, 240, 240, 240)
			//	});
			//}
		}
#endif
	}
}

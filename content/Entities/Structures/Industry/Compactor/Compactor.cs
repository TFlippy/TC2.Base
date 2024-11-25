namespace TC2.Base.Components
{
	public static partial class Compactor
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0
			}

			public Compactor.Data.Flags flags;
			[Editor.Picker.Position(true, true)] public Vector2 offset;
			[Save.Ignore, Net.Ignore] public float t_next_update;
		}

		public const float update_interval = 0.20f;

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] ref Compactor.Data compactor)
		{

		}

		[IEvent.Data]
		public struct DetonateEvent(): IEvent,
		IEvent<Crafting.ConfiguredStage>, IDrawable<Crafting.Context>
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,

				//Unlimited = 1 << 0,
			}

			public Pressure pressure;
			public Temperature temperature;
			public Volume volume;

			public Resource.Data resource_explosive;

			public Sound.Handle h_sound;
			public float sound_volume = 1.00f;
			public float sound_pitch = 1.00f;
			public float sound_size = 4.00f;
			public float sound_dist_modifier = 1.00f;

			public float shake_radius = 32.00f;
			public float shake_power = 0.75f;

			public Compactor.DetonateEvent.Flags flags;

			//void IEvent<Crafting.ConfiguredRecipe>.Bind(ref Crafting.ConfiguredRecipe arg)
			//{
			//	this.temperature = arg.context.temperature;

			//	ref var prd_explosive = ref arg.products.GetFirstOrNull((x) => x.type == Crafting.Product.Type.Resource && x.flags.HasAny(Crafting.Product.Flags.Parameter));
			//	if (prd_explosive.IsNotNull())
			//	{
			//		this.resource_explosive = new(prd_explosive.material, prd_explosive.GetProductAmount(arg.amount_multiplier).GetSum());
			//	}

			//	//App.WriteLine($"Bind {this.ent_parent}; {this.h_substance}; {this.h_prefab}; {this.mass}");
			//}

			void IEvent<Crafting.ConfiguredStage>.Bind(ref Crafting.ConfiguredStage arg)
			{
				ref var req_explosive = ref arg.requirements.GetFirstOrNull((x) => x.type == Crafting.Requirement.Type.Resource && x.flags.HasAny(Crafting.Requirement.Flags.Argument));
				if (req_explosive.IsNotNull())
				{
					this.resource_explosive = new(req_explosive.material, req_explosive.GetRequirementAmount(arg.amount_multiplier).amount);
				}

				//App.WriteLine($"Bind {this.ent_parent}; {this.h_substance}; {this.h_prefab}; {this.mass}");
			}


#if CLIENT
			void IDrawable<Crafting.Context>.OnDraw(ref Crafting.Context arg, GUI.Group group)
			{
				GUI.NewLine(3);

				using (var group_row = GUI.Group.New(size: new(GUI.RmX, 32), padding: new(6, 4)))
				{
					group_row.DrawBackground(GUI.tex_panel);

					//GUI.TitleCentered("Form Type:"u8, pivot: new(0, 0), offset: new(4, 0));
					GUI.TitleCentered($"> Detonation ({this.resource_explosive.GetMass():0.00} kg {this.resource_explosive.GetName()}) <", size: 20, pivot: new(0.50f, 0.50f), color: GUI.col_button_yellow);
				}
			}
#endif
		}

#if SERVER
		[ISystem.Event<Compactor.DetonateEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 500)]
		public static void OnDetonateEvent(ISystem.Info info, ref Region.Data region, Entity entity, ref XorRandom random, [Source.Owned] ref Compactor.DetonateEvent ev,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Health.Data health,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned, Optional(true)] ref Air.Container.Data air_container,
		[Source.Owned] ref Compactor.Data compactor)
		{
			//var record = ent.GetRecord();

			App.WriteLine($"Compactor OnDetonateEvent {ev.resource_explosive.material.GetIdentifier()} {ev.resource_explosive.quantity:0.00}");
			
			ref var material_explosive = ref ev.resource_explosive.GetMaterial();
			if (material_explosive.IsNotNull())
			{
				var pos = transform.LocalToWorld(crafter.spawn_offset);
				var purity = material_explosive.purity;
				var explosive_mass = ev.resource_explosive.GetMassNormalized();

				var (gas_mass, particulates_mass) = explosive_mass.Split(purity);

				Shake.Emit(region: ref region, world_position: pos, trauma: ev.shake_power, max: 1.00f, radius: ev.shake_radius);
				Sound.Play(region: ref region, sound: ev.h_sound, world_position: pos, volume: ev.sound_volume, pitch: ev.sound_pitch, size: ev.sound_size, priority: 0.75f, dist_multiplier: ev.sound_dist_modifier);

				// TODO: use material's fuel properties
				if (air_container.IsNotNull())
				{
					var tmp = gas_mass;
					var blob = new Air.Blob(Air.Composition.FromMass((tmp.SplitRef(0.15f), 0.00f, tmp.SplitRef(0.75f), tmp.SplitRef(0.15f), 0.00f, tmp.SplitRef(0.50f), tmp.SplitRef(0.50f), tmp)), 750.00f);
					air_container.AddBlob(in blob);

					var (mass_soot, mass_ash) = particulates_mass.Split(random.NextFloat01(purity));
					air_container.particulates += new Air.Particulates(mass_ash, mass_soot, 0.00f, 0.00f);

					air_container.Sync(entity);
				}
			}
		}
#endif

#if CLIENT
		public struct CompactorGUI: IGUICommand
		{
			public Entity ent_compactor;

			public Transform.Data transform;
			public Crafter.Data crafter;
			public Crafter.State crafter_state;
			public Compactor.Data compactor;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Compactor"u8, this.ent_compactor, size: new(24, 96 * 1)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{

						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Compactor.Data compactor)
		{
			if (interactable.IsActive())
			{
				var gui = new CompactorGUI()
				{
					ent_compactor = entity,

					transform = transform,
					crafter = crafter,
					crafter_state = crafter_state,
					compactor = compactor
				};
				gui.Submit();
			}
		}
#endif
	}
}

namespace TC2.Base.Components
{
	public static partial class Fillable
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0,
				Reusable = 1 << 1,
			}

			public Fillable.Data.Flags flags;
			public Material.Form form_type;

			public float max_tilt = 1.00f;

			public ISubstance.Handle h_substance_fill;
			public ISubstance.Handle h_substance_mold;

			public Volume volume_capacity;
			public Volume volume_current;

			public Mass mass_current;

			[Obsolete] public float capacity;
			[Obsolete] public float amount;

			public Sound.Handle h_sound_dump;
			public Sound.Handle h_sound_fill;

			[Editor.Picker.Position(true, mark_modified: true)] public Vector2 offset_drop;

			public float tilt_ratio;
			[Save.Ignore, Net.Ignore] public float t_next_update;

			public static bool draw_debug = false;
		}

		//[IComponent.Data(Net.SendType.Unreliable)]
		//public struct State: IComponent
		//{
		//	[Flags]
		//	public enum Flags: uint
		//	{
		//		None = 0,
		//	}

		//	public Volume capacity;
		//	public Volume amount;

		//	[Save.Ignore, Net.Ignore] public float t_next_update;

		//	public State()
		//	{

		//	}
		//}

		[IEvent.Data]
		public struct ModifyEvent(): IEvent, IEvent<Crafting.ConfiguredRecipe>, IDrawable<Crafting.ConfiguredRecipe>
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,
			}

			public Material.Form? form_type;
			public ModifyEvent.Flags? flags;

			public ISubstance.Handle? h_substance_mold;
			public float? quality;
			public float? capacity;

			void IEvent<Crafting.ConfiguredRecipe>.Bind(ref Crafting.ConfiguredRecipe arg)
			{
				ref var prd_substance = ref arg.products.GetFirstOrNull((x) => x.type == Crafting.Product.Type.Substance && x.flags.HasAny(Crafting.Product.Flags.Parameter));
				if (prd_substance.IsNotNull())
				{
					this.h_substance_mold = prd_substance.h_substance;
				}
			}

#if CLIENT
			void IDrawable<Crafting.ConfiguredRecipe>.OnDraw(ref Crafting.ConfiguredRecipe arg, GUI.Group group)
			{
				GUI.NewLine(3);

				using (var group_row = GUI.Group.New(size: new(GUI.RmX, 64), padding: new(8, 4)))
				{
					group_row.DrawBackground(GUI.tex_panel);

					//GUI.TitleCentered("Form Type:"u8, pivot: new(0, 0), offset: new(4, 0));
					GUI.Title("Casting Mold Shape"u8);

					GUI.NewLine(2);

					GUI.LabelShaded("Shape:"u8, this.form_type, width: 112);
					GUI.SameLine();
					GUI.TextShaded(", "u8);
					GUI.SameLine();
					GUI.TextShaded(this.capacity ?? 0.00f, format: "0'x'");
					
					// GUI.SameLine(32);
					// GUI.LabelShaded("Capacity:", this.capacity, format: "0'x'", width: 96);
					GUI.LabelShaded("Material:"u8, this.h_substance_mold.GetName().OrDefault("<none>"), width: 140);

					//GUI.TextShaded("Mold Shape: "u8);
					//GUI.SameLine();
					//GUI.TextShaded(this.form_type);
					//GUI.SameLine();
					//GUI.TextShaded(", "u8);
					//GUI.SameLine();
					//GUI.TextShaded(this.capacity, format: "0'x'");

					//GUI.LabelShaded("- ", this.form_type, width: 108);

					//GUI.TextShaded(","u8);
					//GUI.LabelShaded("", this.form_type, font_a: GUI.Font.Superstar, size_a: 16, width: 128);
				}
			}
#endif
		}

#if SERVER
		[ISystem.Event<Fillable.ModifyEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 10)]
		public static void OnModifyEvent(ISystem.Info info, ref Region.Data region, Entity entity, ref Fillable.ModifyEvent ev, ref XorRandom random,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] ref Fillable.Data fillable)
		{
			// App.WriteLine("on modify fillable event");

			fillable.form_type.SetIfHasValue(ev.form_type);
			fillable.h_substance_mold.SetIfHasValue(ev.h_substance_mold);
			fillable.capacity.SetIfHasValue(ev.capacity);
			fillable.amount.SetIfHasValue(0.00f);

			fillable.Sync(entity);
		}
#endif

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateTilt(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] ref Fillable.Data fillable)
		{
			var time = info.WorldTime;

			var pos = transform.LocalToWorld(fillable.offset_drop);
			var rot = transform.rotation;
			var rot_up = transform.rotation - Maths.half_pi;

			var tilt_delta = Maths.DeltaAngle(rot_up, -Maths.half_pi);
			var tilt_delta_abs = tilt_delta.Abs();
			var tilt_ratio = Maths.Normalize01Fast(tilt_delta_abs, fillable.max_tilt);

			fillable.tilt_ratio = tilt_ratio;

#if SERVER
			if (time >= fillable.t_next_update)
			{
				fillable.t_next_update = time + random.NextFloatExtra(0.30f, 0.85f);
				if (fillable.tilt_ratio >= 1.00f)
				{
					if (fillable.amount > 0.00f && fillable.h_substance_fill != default)
					{
						ref var substance_data = ref fillable.h_substance_fill.GetData();
						if (substance_data.IsNotNull())
						{
							ref var material_data = ref substance_data.GetMaterial(fillable.form_type, out var material_asset);
							if (material_data.IsNotNull())
							{
								fillable.amount.SubTowards(0.00f, Maths.Ceil(fillable.amount * random.NextFloatExtra(0.20f, 0.45f)), out var amount_spawn);
								region.SpawnResource(material_asset, pos, amount_spawn, max_distance: 1.30f, flags: Resource.SpawnFlags.Merge, angular_velocity: random.NextFloat(1.20f), velocity: Maths.RadToDir(rot_up) * random.NextFloatExtra(1.00f, 6.50f), temperature: heat_state.temperature_current, rotation: rot);
								Sound.Play(ref region, fillable.h_sound_dump, pos, volume: random.NextFloatExtra(0.45f, 0.25f), pitch: random.NextFloatExtra(0.35f, 0.38f), size: 1.00f, dist_multiplier: 0.80f);

								if (fillable.amount <= 0.01f)
								{
									fillable.h_substance_fill = default;
									fillable.amount = 0.00f;
								}

								fillable.Sync(entity);
							}
						}
					} 
				}
			}
#endif

#if CLIENT
			if (Fillable.Data.draw_debug)
			{
				region.DrawDebugArc(pos, -Maths.half_pi - fillable.max_tilt, -Maths.half_pi + fillable.max_tilt, 2, color: Color32BGRA.Orange);
				region.DrawDebugDir(pos, Maths.RadToDir(rot_up), color: Color32BGRA.Yellow);

				region.DrawDebugText(pos, $"{tilt_delta:0.00}", color: Color32BGRA.White);
			}
#endif
		}

#if SERVER
		[ISystem.Event<Blob.ContactEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 50)]
		public static void OnContactBlob(ISystem.Info info, ref Region.Data region, Entity ent_fillable, ref Blob.ContactEvent ev,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform,
		//[Source.Owned, Pair.Component<Fillable.Data>] ref Inventory4.Data inventory,
		[Source.Owned] ref Fillable.Data fillable)
		{
			if (fillable.amount >= fillable.capacity || fillable.tilt_ratio >= 1.00f) return;
			//App.WriteLine("fillable: on contact blob");

			var h_substance = ev.blob.h_substance;
			ref var substance_data = ref h_substance.GetData();
			if (substance_data.IsNotNull())
			{
				ref var material_data = ref substance_data.GetMaterial(fillable.form_type, out var material_asset);
				if (material_data.IsNotNull())
				{
					var h_material = material_asset.GetHandle();
					var amount_req = fillable.capacity - fillable.amount;
					if (amount_req > 0.00f)
					{
						var mass_req = (Mass)(amount_req * material_data.mass_per_unit);
						var mass_add = (Mass)Maths.Min(mass_req, ev.mass);

						// App.WriteLine($"{mass_add}/{mass_req}/{ev.mass}");

						if (fillable.h_substance_fill == default || fillable.amount < 0.01f) fillable.h_substance_fill = h_substance;

						if (fillable.h_substance_fill == h_substance)
						{
							var resource_tmp = new Resource.Data(h_material, fillable.amount);
							if (Resource.TryAddHeatedMass(ref resource_tmp, ref heat_state.temperature_current, h_substance, ref mass_add, ref ev.temperature, out var amount_added))
							{
								fillable.amount += amount_added;
								ev.mass -= amount_added * material_data.mass_per_unit;
								fillable.Sync(ent_fillable, true);
							}
						}
						else // slag
						{

						}
					}
				}
			}
		}
#endif

#if CLIENT
		public struct FillableGUI: IGUICommand
		{
			public Entity ent_fillable;

			public Transform.Data transform;
			public Fillable.Data fillable;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Fillable"u8, this.ent_fillable, size: new(48, 48 * 4), min_width: 48, min_height: 48))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: GUI.Rm))
						{
							var color_fill = Color32BGRA.Neutral;

							ref var substance_fill_data = ref this.fillable.h_substance_fill.GetData();
							if (substance_fill_data.IsNotNull())
							{
								color_fill = substance_fill_data.color_gui;
							}

							GUI.DrawVerticalGauge(this.fillable.amount, this.fillable.capacity, size: GUI.Rm, color: color_fill);
							GUI.DrawHoverTooltip(in this.fillable, static (x) =>
							{
								GUI.Title("Casting Mould"u8);
								//GUI.TextShaded(x.arg.h_substance_fill.GetName().OrDefault("<none>"));
								GUI.LabelShaded("Capacity:"u8, x.arg.capacity, format: $"0'x {x.arg.form_type}'", width: 128);
								GUI.LabelShaded("Amount:"u8, x.arg.amount, format: $"0'x {x.arg.form_type}'", width: 128);
							
								//if (x.arg.h_substance_fill != 0) GUI.LabelShaded(x.arg.h_substance_fill.GetName(), Maths.Normalize01(x.arg.amount, x.arg.capacity) * 100.00f, format: "0'%'");
								if (x.arg.h_substance_fill != 0) GUI.LabelShaded(x.arg.h_substance_fill.GetName(), x.arg.amount * x.arg.h_substance_fill.GetMaterialHandle(x.arg.form_type).GetMassPerUnit(), format: "0.##'kg'");
							});
						}
					}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region, order: 350)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Fillable.Data fillable)
		{
			if (interactable.IsActive())
			{
				var gui = new FillableGUI()
				{
					ent_fillable = entity,

					transform = transform,
					fillable = fillable
				};
				gui.Submit();
			}
		}
#endif
	}
}

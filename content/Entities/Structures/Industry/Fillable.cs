namespace TC2.Base.Components
{
	public static partial class Fillable
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0,

			}

			public Fillable.Data.Flags flags;
			public Material.Form form_type;

			public float max_tilt = 1.00f;

			public ISubstance.Handle h_substance_fill;
			public float capacity;
			public float amount;

			public Sound.Handle h_sound_dump;
			public Sound.Handle h_sound_fill;

			[Editor.Picker.Position(true, mark_modified: true)] public Vector2 offset_drop;

			public float tilt_ratio;
			[Save.Ignore, Net.Ignore] public float t_next_update;

			public Data()
			{

			}

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

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateTilt(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
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

				region.DrawDebugText(pos, $"{tilt_delta:0.00}", color:  Color32BGRA.White);
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

						App.WriteLine($"{mass_add}/{mass_req}/{ev.mass}");

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
				using (var window = GUI.Window.InteractionMisc("Fillable"u8, this.ent_fillable, size: new(24, 96 * 1)))
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

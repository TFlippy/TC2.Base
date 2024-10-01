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

				Use_Misc = 1 << 0
			}

			public Fillable.Data.Flags flags;
			public Material.Form form_type;

			public float capacity;
			public float amount;

			[Save.Ignore, Net.Ignore] public float t_next_update;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.Event<Blob.ContactEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 50)]
		public static void OnContactBlob(ISystem.Info info, ref Region.Data region, Entity ent_fillable, ref Blob.ContactEvent ev,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform,
		//[Source.Owned, Pair.Component<Fillable.Data>] ref Inventory4.Data inventory,
		[Source.Owned] ref Fillable.Data fillable)
		{
			App.WriteLine("fillable: on contact blob");

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

						var resource_tmp = new Resource.Data(h_material, fillable.amount);
						if (Resource.TryAddHeatedMass(ref resource_tmp, ref heat_state.temperature_current, h_substance, ref mass_add, ref ev.temperature, out var amount_added))
						{
							fillable.amount += amount_added;
							ev.mass -= mass_add;

							fillable.Sync(ent_fillable, true);
						}
					}
				}
			}
		}
#endif

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Fillable.Data fillable)
		{

		}

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

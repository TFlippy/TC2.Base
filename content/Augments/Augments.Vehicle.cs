using Keg.Engine.Game;
using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod
	{
		private static void RegisterVehicleAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Tractor.Data>
			(
				identifier: "tractor.brake",
				category: "Tractor",
				name: "Brake",
				description: "",

				can_add: static (ref Augment.Context context, in Tractor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Tractor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					return GUI.SliderFloatLerp("Multiplier", ref modifier, 0.50f, 1.50f, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Tractor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					data.brake_step *= Maths.Lerp(0.50f, 1.50f, modifier);
				},

				apply_1: static (ref Augment.Context context, ref Tractor.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100, 10));
				}
			));

			definitions.Add(Augment.Definition.New<Mount.Data>
			(
				identifier: "mount.trajectory",
				category: "Mount",
				name: "Trajectory Calculator",
				description: "",

				can_add: static (ref Augment.Context context, in Mount.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !data.flags.HasAny(Mount.Flags.Show_Trajectory);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Mount.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					return GUI.SliderFloatLerp("Multiplier", ref modifier, 0.50f, 1.50f, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Mount.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					data.flags.AddFlag(Mount.Flags.Show_Trajectory);
				},

				apply_1: static (ref Augment.Context context, ref Mount.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("computer", 1.00f));
					context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 15.00f));
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 500, 20));
				}
			));
		}
	}
}


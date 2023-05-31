using Keg.Engine.Game;
using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod
	{
		private static void RegisterWrenchAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Holdable.Data>
			(
				identifier: "holdable.wrench",
				category: "Utility",
				name: "Add: Wrench",
				description: "Allows the item to act as a wrench.",

				can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.flags.HasAll(Holdable.Flags.Storable) && !data.flags.HasAny(Holdable.Flags.Disable_Control | Holdable.Flags.Disable_Parent_Facing) && !augments.HasAugment(handle) && !context.HasComponent<Wrench.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					var size = GUI.GetRemainingSpace();
					size.X *= 0.50f;

					var dirty = false;
					dirty |= GUI.Picker("offset", "Offset", size: size, ref offset, min: new Vector2(-1.00f, -1.00f), max: new Vector2(1.00f, 1.00f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					draw.DrawSprite("augment.wrench", new Vector2(offset.X, offset.Y), scale: new(offset.X > 0.00f ? 1.00f : -1.00f, offset.Y > 0.00f ? -1.00f : 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var wrench = ref context.GetOrAddComponent<Wrench.Data>();
					
					ref var interactable = ref context.GetOrAddComponent<Interactable.Data>();
					if (interactable.IsNotNull())
					{
						interactable.window_size = Vector2.Max(interactable.window_size, new Vector2(422, 500));
					}
				},

				apply_1: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100, 10));
				}
			));

			definitions.Add(Augment.Definition.New<Wrench.Data>
			(
				identifier: "wrench.belts",
				category: "Wrench",
				name: "Mode: Belts",
				description: "Adds belt-manipulation functionality.",

				can_add: static (ref Augment.Context context, in Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Wrench.Mode.Belts.Data>();
				},

				apply_0: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var mode = ref context.GetOrAddComponent<Wrench.Mode.Belts.Data>();
				},

				apply_1: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100, 10));
				}
			));

			definitions.Add(Augment.Definition.New<Wrench.Data>
			(
				identifier: "wrench.build",
				category: "Wrench",
				name: "Mode: Build",
				description: "Adds building functionality.",

				can_add: static (ref Augment.Context context, in Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Wrench.Mode.Build.Data>();
				},

				apply_0: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var mode = ref context.GetOrAddComponent<Wrench.Mode.Build.Data>();
				},

				apply_1: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100, 10));
				}
			));

			definitions.Add(Augment.Definition.New<Wrench.Data>
			(
				identifier: "wrench.deconstruct",
				category: "Wrench",
				name: "Mode: Deconstruct",
				description: "Adds deconstruction functionality.",

				can_add: static (ref Augment.Context context, in Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Wrench.Mode.Deconstruct.Data>();
				},

				apply_0: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var mode = ref context.GetOrAddComponent<Wrench.Mode.Deconstruct.Data>();
				},

				apply_1: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100, 10));
				}
			));

			definitions.Add(Augment.Definition.New<Wrench.Data>
			(
				identifier: "wrench.conveyors",
				category: "Wrench",
				name: "Mode: Conveyors",
				description: "Adds conveyor-manipulation functionality.",

				can_add: static (ref Augment.Context context, in Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Wrench.Mode.Conveyors.Data>();
				},

				apply_0: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var mode = ref context.GetOrAddComponent<Wrench.Mode.Conveyors.Data>();
				},

				apply_1: static (ref Augment.Context context, ref Wrench.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100, 10));
				}
			));
		}
	}
}


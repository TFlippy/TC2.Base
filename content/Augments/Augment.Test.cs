using Keg.Engine.Game;
using TC2.Base.Components;

namespace TC2.Base.Augments
{
	[Augment(identifier: "casing.steel", name: "Steel Casing", description: "TODO: Desc", category: "Misc")]
	public struct SteelCasingAugment: IAugment3<SteelCasingAugment>
	{
		public enum Type: byte
		{
			Undefined = 0
		}

		public Vector2 offset;
		public SteelCasingAugment.Type type;

		public static Texture.Handle tex_sprite = "augment.casing.steel";

		public static Augment.CanAdd<SteelCasingAugment> CanAdd { get; } = static (args) => args.augments.GetCount(args.handle) < 4;
		public static Augment.Validate<SteelCasingAugment> Validate { get; } = null;
		public static Augment.Apply<SteelCasingAugment> Apply { get; } = static (args) =>
		{
			App.WriteLine("hello");
		};
		public static Augment.Requirements<SteelCasingAugment> Requirements { get; } = null;
		public static Augment.Finalize<SteelCasingAugment> Finalize { get; } = null;

#if CLIENT
		public static Augment.DrawEditor<SteelCasingAugment> DrawEditor { get; } = static (args) =>
		{
			var dirty = false;

			dirty |= GUI.EnumInput("type", ref args.data.type, size: new Vector2(GUI.RmX * 0.50f, GUI.RmY), show_label: false, close_on_select: false);
			GUI.SameLine();
			dirty |= GUI.Picker("offset", "Offset", size: GUI.Rm, ref args.data.offset, min: args.context.rect.a, max: args.context.rect.b);

			return dirty;
		};

		public static Augment.GenerateSprite<SteelCasingAugment> GenerateSprite { get; } = static (args) =>
		{
			var sprite = new Sprite(tex_sprite, 24, 16, (uint)args.data.type, 0);
			args.draw.DrawSprite(sprite, args.data.offset, pivot: new(0.50f, 0.50f));
		};
#endif
	}
}



namespace TC2.Base.Components
{
	[Asset.Hjson(prefix: "catalogue.", capacity_world: 512, capacity_region: 16, capacity_local: 8, flags_world: Asset.Flags.Recycle, flags_local: Asset.Flags.Recycle)]
	public interface ICatalogue: IAsset2<ICatalogue, ICatalogue.Data>
	{
		[Net.MsgPack]
		public struct Data(): IName
		{
			[Net.Key(00), Save.Force] public string name;

			//[Net.Key(00)]
			[Save.NewLine]
			[Net.Key(02), Save.Force] public ICompany.Handle h_company;

			[Save.NewLine]
			[Net.Key(04), Save.Force] public ImperialDateTime date_created;

			[Save.NewLine]
			[Net.Key(05), Save.Force, Save.Inline] public Shipment.Item2[] items;
			//[Net.Key(06), Save.Force, Save.Inline] public Shipment.Item2[] items_trading;

			readonly ReadOnlySpan<char> IName.GetName() => this.name;
			readonly ReadOnlySpan<char> IName.GetShortName() => this.name;
		}
	}
}

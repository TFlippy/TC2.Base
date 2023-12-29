
//namespace TC2.Base.Components
//{
//	public static partial class MoneyBag
//	{
//#if SERVER
//		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
//		public static void OnConsume(ISystem.Info info, ref Region.Data region, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable)
//		{
//			if (data.ent_holder.IsAlive())
//			{
//				var oc_money_holder = data.ent_holder.GetComponentWithOwner<Money.Data>(Relation.Type.Instance, true);
//				ref var money_holder = ref oc_money_holder.data;
//				if (money_holder.IsNotNull())
//				{
//					var amount = Maths.Max(0.00f, money.amount);
//					money_holder.amount += amount;

//					WorldNotification.Push(ref region, $"* Takes {amount:0.00} {Money.symbol} *", Color32BGRA.Green, data.world_position, lifetime: 1.50f);

//					money.amount = 0.00f;

//					oc_money_holder.Sync(true);
//					money.Sync(data.ent_consumable, true);
//				}
//			}
//		}

//		[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
//		public static void OnRemoveCharacter(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_character, Entity ent_money, Entity ent_npc,
//		[Source.Shared] ref Character.Data character, [Source.Shared] ref Money.Data money,
//		[Source.Owned, Original] ref NPC.Data npc, [Source.Owned] in Transform.Data transform)
//		{
//			var amount = money.amount;
//			if (amount >= 10.00f)
//			{
//				money.amount = 0.00f;
//				money.Sync(ent_money, true);

//				region.SpawnPrefab("money.00", transform.position + random.NextUnitVector2Range(0.125f, 0.500f)).ContinueWith((ent) =>
//				{
//					ref var money_new = ref ent.GetComponent<Money.Data>();
//					if (money_new.IsNotNull())
//					{
//						money_new.amount = amount;
//						money_new.Sync(ent, true);
//					}
//				});
//			}
//		}
//#endif
//	}
//}
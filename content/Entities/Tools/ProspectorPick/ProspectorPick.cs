
namespace TC2.Base.Components
{
	[Shitcode]
	public static partial class ProspectorPick
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public float max_depth = 8.00f;
			[Save.Ignore, Net.Ignore] public float next_hit;

			[Save.NewLine]
			[Asset.Ignore] public Vector2 position;
			[Asset.Ignore] public Vector2 direction;

			[Save.NewLine]
			[Asset.Ignore] public FixedArray8<OreSample> samples;
		}

		//[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		//public partial struct State(): IComponent
		//{

		//}

		public struct OreSample
		{
			public IBlock.Handle block;
			public float quantity;
		}

#if CLIENT
		[Shitcode]
		public struct ProspectorPickGUI: IGUICommand
		{
			public Entity ent_pick;
			public Transform.Data transform;
			public Control.Data control;
			public ProspectorPick.Data prospector_pick;
			//public ProspectorPick.State prospector_pick_state;

			public void Draw()
			{
				ref var region = ref Client.GetRegion();
				ref var terrain = ref region.GetTerrain();

				var pos = region.WorldToCanvas(this.prospector_pick.position);

				var a = region.WorldToCanvas(this.prospector_pick.position + (this.prospector_pick.direction * -3.00f));
				var b = region.WorldToCanvas(this.prospector_pick.position);
				var c = region.WorldToCanvas(this.prospector_pick.position + (this.prospector_pick.direction * this.prospector_pick.max_depth));

				using (var window = GUI.Window.HUD("Prospector Pick"u8, position: a + new Vector2(0.00f, -2.00f), size: new(168, 0), pivot: new(0.50f, 1.00f)))
				{
					if (window.show)
					{
						ref var samples = ref this.prospector_pick.samples;

						var total_count = 0.00f;
						for (var i = 0; i < samples.Length; i++)
						{
							ref var sample = ref samples[i];
							total_count += sample.quantity;
						}

						if (total_count > Maths.epsilon)
						{
							//GUI.DrawLine(a, b, GUI.font_color_default, 1.00f);
							GUI.DrawLine2(a, b - (this.prospector_pick.direction * 0.25f * region.GetWorldToCanvasScale()), GUI.font_color_default, GUI.font_color_default.WithAlphaDiv(Maths.Factor.x2), 4.00f, 1.00f);

							GUI.DrawLine(a - new Vector2(80, 0), a + new Vector2(80, 0), GUI.font_color_default, 1.00f);
							//GUI.DrawLine2(b, c, GUI.font_color_default.WithAlphaMult(0.25f), GUI.font_color_default.WithAlphaMult(0.00f), 4.00f, 1.00f);
							GUI.DrawCircleFilled(b, 0.075f * region.GetWorldToCanvasScale(), GUI.font_color_default.WithAlphaMult(0.80f));

							GUI.Title("Composition"u8);

							for (var i = 0; i < samples.Length; i++)
							{
								ref var sample = ref samples[i];
								if (sample.block)
								{
									ref var block = ref sample.block.GetData();

									var ratio = sample.quantity.Normalize01Fast(total_count); // / total_count;
									var color = Color32BGRA.RedGreen(ratio); //.FromHSV(ratio * 2.00f, 1.00f, 1.00f);

									//if (block.flags.HasAny(Block.Flags.Rare | Block.Flags.Uncommon)) color = GUI.font_color_default;
									//else if (block.flags.HasAll(Block.Flags.Common)) color = GUI.font_color_default;
									//else color = GUI.font_color_default.WithColorMult(0.50f);

									GUI.LabelShaded(block.name, ratio, "P0", color_a: color, color_b: GUI.font_color_default);
								}
							}

							var offset = region.WorldToCanvas(Vector2.Min(this.prospector_pick.position, this.prospector_pick.position + this.prospector_pick.direction * this.prospector_pick.max_depth));

							var random = XorRandom.New((uint)(App.GetFixedTime() * 15.00));

							var alpha = 1.00f; // (this.prospector_pick.next_hit - region.GetWorldTime()); //.Clamp0X();

							var rect_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();
							var args = (offset: offset, rect_size: rect_size, random: random, alpha: alpha);

							terrain.IterateLine(this.prospector_pick.position, this.prospector_pick.position + this.prospector_pick.direction * this.prospector_pick.max_depth, 0.50f, ref args, Func, iteration_flags: Terrain.IterationFlags.None);
							static void Func(Tile tile, ref (Vector2 offset, Vector2 rect_size, XorRandom random, float alpha) args, int x, int y, byte mask)
							{
								var h_block = tile.GetBlockHandle();
								if (h_block && h_block.GetFlags().HasAnyExcept(Block.Flags.Mineral | Block.Flags.Ore | Block.Flags.Native | Block.Flags.Rock | Block.Flags.Metal | Block.Flags.Raw, Block.Flags.Artificial | Block.Flags.Masonry | Block.Flags.Construction | Block.Flags.Overgrown))
								{
									//ref var block = ref h_block.get
									//if (args.random.NextBool(0.05f) && block.flags.HasAny(Block.Flags.Mineral | Block.Flags.Ore))

									ref var block_data = ref h_block.GetData();
									if (block_data.IsNotNull() && args.random.NextBool(0.10f))
									{
										var pos = args.offset + new Vector2(args.rect_size.X * x, args.rect_size.Y * y); // bleh
										GUI.DrawRectFilled(a: pos - new Vector2(args.random.NextFloatRange(-8.50f, 15.00f)), b: pos + new Vector2(args.random.NextFloatRange(-20.50f, 15.00f)), 
											color: block_data.color_preview.WithAlphaMult(args.alpha * args.random.NextFloatRange(0.10f, 0.50f)));
									}
								}
							}

							//GUI.Separator();

							//GUI.LabelShaded("Richness:", total_count);
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] in ProspectorPick.Data prospector_pick, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control,
		[Source.Parent] in Player.Data player)
		{
			var gui = new ProspectorPickGUI()
			{
				ent_pick = entity,
				transform = transform,
				control = control,
				prospector_pick = prospector_pick
			};
			gui.Submit();
		}
#endif

#if SERVER
		[ISystem.Event<Melee.HitEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnHit(ISystem.Info info, Entity entity, ref Region.Data region, ref Melee.HitEvent data, 
		[Source.Owned] ref ProspectorPick.Data prospector_pick)
		{
			ref var terrain = ref region.GetTerrain();

			var arg = (a: 0, samples: new FixedArray8<OreSample>());

			terrain.IterateLine(world_position_a: data.world_position, world_position_b: data.world_position + data.direction * prospector_pick.max_depth, thickness: 0.10f, argument: ref arg, func: Func, iteration_flags: Terrain.IterationFlags.None);
			static void Func(Tile tile, ref (int a, FixedArray8<OreSample> samples) arg, int x, int y, byte mask)
			{
				var h_block = tile.GetBlockHandle();
				if (h_block && h_block.GetFlags().HasAnyExcept(Block.Flags.Mineral | Block.Flags.Ore | Block.Flags.Native | Block.Flags.Rock | Block.Flags.Metal | Block.Flags.Raw, Block.Flags.Artificial | Block.Flags.Masonry | Block.Flags.Construction | Block.Flags.Overgrown))
				{
					for (var i = 0; i < arg.samples.Length; i++)
					{
						ref var sample = ref arg.samples[i];
						if (!sample.block || sample.block == h_block)
						{
							sample.block = h_block;
							sample.quantity += 1.00f;

							break;
						}
					}
				}
			}

			arg.samples.AsSpan().Sort((x, y) => y.quantity.CompareTo(x.quantity));

			prospector_pick.position = data.world_position;
			prospector_pick.direction = data.direction;
			prospector_pick.samples = arg.samples;
			prospector_pick.Sync(entity);
		}
#endif
	}
}

namespace TC2.Base.Components
{
	public static class Chisel
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,
			}

			public float power;
			public float size;

			public Chisel.Data.Flags flags;

			[Save.Ignore, Net.Ignore] public float t_next_use;
		}

		public struct UseRPC: Net.IRPC<Chisel.Data>
		{
			public Vector2 world_position;
			public Vector2 direction;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Chisel.Data data)
			{
				ref var region = ref rpc.GetRegion();
				ref var terrain = ref region.GetTerrain();

				var rect = Terrain.GetGridRect(this.world_position, new Vector2(0.125f));

				//var size = new Vector2(1.00f);
				//var size_half = size * 0.50f;

				//var w_pos_new = this.world_position;
				//var w_pos_new_snapped = w_pos_new.SnapFloor(0.125f);
				//var bb = AABB.Simple(w_pos_new_snapped, new Vector2(0.125f));
				//var bb_outside = bb.Grow(0.50f); // AABB.Simple(w_pos_new_snapped, new Vector2(0.125f));

				//var color = Color32BGRA.Yellow;
				//var color_ok = Color32BGRA.Green;
				//var c_pos_new_snapped = region.WorldToCanvas(w_pos_new_snapped);


				//var bb = AABB.Simple(w_pos_new_snapped - size_half, size + new Vector2(0.125f));

				var tile_flags = TileFlags.Non_Empty;
				//var pixel_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();

				//GUI.DrawChunkRect(ref region, w_pos_new);

				terrain.Hit(rpc.entity, rpc.entity, rect: rect, world_position: this.world_position, direction: this.direction, damage: 100000, yield: 0.00f, mask: TileFlags.Non_Empty, no_loot_pickup: true);
				//region.DrawDebugRect(rect, Color32BGRA.Magenta);

				//{
				//	var args = new DrawTileArgs(offset: region.WorldToCanvas(bb_outside.a), pixel_size: pixel_size, color: color.WithAlpha(100), tile_flags: tile_flags);
				//	terrain.IterateRect(bb_outside, argument: ref args,
				//		func: DrawTileFunc,
				//		iteration_flags: Terrain.IterationFlags.Iterate_Empty);
				//}

				//GUI.DrawTerrainOutline(ref region, w_pos_new, radius: 1.00f, thickness: 1.00f, color: GUI.col_button_yellow.WithAlpha(200));

				//{
				//	var args = new DrawTileArgs(offset: region.WorldToCanvas(bb.a), pixel_size: pixel_size, color: color_ok.WithAlpha(200), tile_flags: tile_flags);
				//	terrain.IterateRect(bb, argument: ref args,
				//		func: DrawTileFunc,
				//		iteration_flags: Terrain.IterationFlags.Iterate_Empty);
				//}


				//var sync = false;

				//var positions_span = data.positions.Slice(data.positions_capacity);
				//if (this.index < positions_span.Length)
				//{
				//	positions_span[this.index] = this.position.GetValueOrDefault().SnapFloor(0.125f);
				//	positions_span.Compact();
				//	data.positions_count = (byte)positions_span.Length;

				//	var random = XorRandom.New(true);
				//	Sound.PlayGUI(ref rpc.connection, this.position.HasValue ? data.h_sound_add : data.h_sound_rem, volume: random.NextFloatRange(0.10f, 0.12f), pitch: random.NextFloatRange(0.98f, 1.02f));

				//	sync = true;

				//	//if (this.position.TryGetValue(out var position_value))
				//	//{
				//	//	sync = positions_span.TrySetAtIndex()
				//	//}
				//	//else
				//	//{
				//	//	sync = positions_span.TryRemoveAtResize(this.index);
				//	//}
				//}


				//if (sync)
				//{
				//	data.Sync(rpc.entity);
				//}
			}
#endif
		}


#if CLIENT
		public struct ChiselGUI: IGUICommand
		{
			public Transform.Data transform;
			public Chisel.Data chisel;
			public Entity entity;

			public void Draw()
			{
				using (var hud = GUI.Window.HUD("chisel.hud"u8))
				{
					if (hud.show)
					{
						ref var region = ref this.entity.GetRegion();
						ref var terrain = ref region.GetTerrain();

						var kb = GUI.GetKeyboard();
						var mouse = GUI.GetMouse();
						var canvas_scale = region.GetWorldToCanvasScale();

						Arm.HoverGUI.Hide();

						var size = 0.50f; // new Vector2(0.50f);
						var size_half = size * 0.50f;

						var w_pos_new = mouse.position - new Vector2(0.25f);
						var w_pos_new_snapped = w_pos_new.SnapFloor(0.125f);
						var bb = AABB.Simple(w_pos_new_snapped, new Vector2(0.125f));
						var bb_outside = bb.Grow(0.50f); // AABB.Simple(w_pos_new_snapped, new Vector2(0.125f));

						var color = Color32BGRA.Yellow;
						var color_ok = Color32BGRA.Green;
						var c_pos_new_snapped = region.WorldToCanvas(w_pos_new_snapped);


						//var bb = AABB.Simple(w_pos_new_snapped - size_half, size + new Vector2(0.125f));

						var tile_flags = TileFlags.Non_Empty;
						var pixel_size = new Vector2(App.pixels_per_unit_inv) * region.GetWorldToCanvasScale();

						GUI.DrawChunkRect(ref region, w_pos_new);

						{
							var args = new DrawTileArgs(offset: region.WorldToCanvas(bb_outside.a), pixel_size: pixel_size, color: color.WithAlpha(100), tile_flags: tile_flags);
							terrain.IterateRect(bb_outside, argument: ref args,
								func: DrawTileFunc,
								iteration_flags: Terrain.IterationFlags.Iterate_Empty);
						}

						GUI.DrawTerrainOutline(ref region, w_pos_new, radius: 1.00f, thickness: 1.00f, color: GUI.col_button_yellow.WithAlpha(200));

						{
							var args = new DrawTileArgs(offset: region.WorldToCanvas(bb.a), pixel_size: pixel_size, color: color_ok.WithAlpha(200), tile_flags: tile_flags);
							terrain.IterateRect(bb, argument: ref args,
								func: DrawTileFunc,
								iteration_flags: Terrain.IterationFlags.Iterate_Empty);
						}

						//GUI.DrawRectFilled(region.WorldToCanvas(AABB.Simple(w_pos_new_snapped, new Vector2(0.1250f))), color.WithAlpha(200));
						GUI.DrawCross(region.WorldToCanvas(w_pos_new_snapped + new Vector2(0.000f)), Color32BGRA.White.WithAlpha(50), radius: region.GetWorldToCanvasScale() * 1.000f);
						GUI.DrawCross(region.WorldToCanvas(w_pos_new_snapped + new Vector2(0.125f)), Color32BGRA.White.WithAlpha(50), radius: region.GetWorldToCanvasScale() * 1.000f);

						if (!GUI.IsHovered)
						{
							if (mouse.GetKeyDown(Mouse.Key.Left))
							{
								var rpc = new Chisel.UseRPC()
								{
									world_position = w_pos_new
								};
								rpc.Send(this.entity);
							}
							//else if (mouse.GetKeyDown(Mouse.Key.Right))
							//{
							//	if (nearest_index >= 0)
							//	{
							//		var rpc = new Chisel.MarkRPC()
							//		{
							//			index = (byte)nearest_index,
							//			position = null,
							//		};
							//		rpc.Send(this.entity);
							//	}
							//}
						}
					}
				}
				
			}

			private record struct DrawTileArgs(Vector2 offset, Vector2 pixel_size, TileFlags tile_flags, Color32BGRA color);
			static void DrawTileFunc(ref Tile tile, int x, int y, byte mask, ref DrawTileArgs args)
			{
				var pos = args.offset + new Vector2(args.pixel_size.X * x, args.pixel_size.Y * y);
				var color = args.color;
				


				if (tile.Flags.HasAll(args.tile_flags))
				{
					if (tile.Neighbours == 255) color = Color32BGRA.Red.WithAlpha(50); //.WithAlphaMult()
					GUI.DrawRectFilled(pos, pos + args.pixel_size, color.WithAlphaMult(tile.Flags.HasAny(TileFlags.Solid) ? 1.00f : 0.250f));
				}
				else
				{ 
					//GUI.DrawRectFilled(pos, pos + args.pixel_size, args.color.WithAlphaMult(0.10f));
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Chisel.Data chisel,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control)
		{
			var gui = new ChiselGUI()
			{
				entity = entity,
				transform = transform,
				chisel = chisel
			};
			gui.Submit();
		}
#endif

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
		public static void Update(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] ref Chisel.Data chisel, [Source.Owned] in Transform.Data transform, 
		[Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body)
		{
			if (control.mouse.GetKey(Mouse.Key.Left) && info.WorldTime >= chisel.t_next_use)
			{

			}
		}
#endif
	}
}

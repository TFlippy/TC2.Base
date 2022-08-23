//using Keg.Engine.Infrastructure;
//using System.Numerics;
//using Chipmunk2D;

//namespace Keg.Engine.Game
//{
//	public static class Pickaxe
//	{
//#if CLIENT
//		public static readonly Texture.Handle texture_stone = "StoneGib";
//		public static readonly Texture.Handle texture_smoke = "LargeSmoke";

//		public struct BlockGUI: IGUICommand
//		{
//			public Transform.Data transform;
//			public Vector2 world_position;
//			public Pickaxe.Data pickaxe;
//			public Entity entity;
//			public Specialization.Miner.Data mining;
//			public bool valid;

//			public unsafe void Draw()
//			{
//				ref var region = ref Client.GetRegion();

//				var radius = this.mining.ApplySize(this.pickaxe.radius * App.pixels_per_unit_inv);
//				var c_radius = radius * GUI.GetWorldToCanvasScale();
//				var c_pos = GUI.WorldToCanvas(this.world_position);

//				GUI.DrawTerrainOutline(ref region, this.world_position, radius, GUI.font_color_green);

//				GUI.DrawCircleFilled(c_pos, c_radius, GUI.font_color_green.WithAlphaMult(0.10f), segments: 16);
//				GUI.DrawCircle(c_pos, c_radius, GUI.font_color_green.WithAlphaMult(0.40f), thickness: 1.00f, segments: 16);


//				//var wp = this.world_position;

//				//var radius = this.mining.ApplySize(this.pickaxe.radius);

//				//var pos = this.WorldToCanvas(wp);
//				//var pos_edge = this.WorldToCanvas(wp + new Vector2(radius, 0.00f));
//				//var radius_canvas = MathF.Abs(pos.X - pos_edge.X) / App.pixels_per_unit; 

//				//var draw = ImGui.GetBackgroundDrawList();

//				//var color_bg = ImGui.ColorConvertFloat4ToU32(this.valid ? new(0, 1, 0, 0.10f) : new(1, 0, 0, 0.10f));
//				//var color_fg = ImGui.ColorConvertFloat4ToU32(this.valid ? new(0, 1, 0, 0.40f) : new(1, 0, 0, 0.40f));

//				//draw.AddCircleFilled(pos, radius_canvas, color_bg, 16);
//				//draw.AddCircle(pos, radius_canvas, color_fg, 16, 1.00f);

//				//ref var region = ref this.entity.GetRegion();

//				//var hits_ptr = stackalloc OverlapResult[32];
//				//{
//				//	var hits = new Span<OverlapResult>(hits_ptr, 32);
//				//	if (region.TryOverlapPointAll(wp, radius * 0.50f, ref hits, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static))
//				//	{
//				//		foreach (var hit in hits)
//				//		{
//				//			var shape = hit.info.shape;
//				//			if (shape->klass->type == Chipmunk2D.cpShapeType.CP_SEGMENT_SHAPE)
//				//			{
//				//				var segment = (cpSegmentShape*)shape;

//				//				var shape_a = this.WorldToCanvas(segment->ta);
//				//				var shape_b = this.WorldToCanvas(segment->tb);

//				//				var dist = hit.distance;

//				//				ref var block = ref NetRegistry<Block.Definition>.Get(segment->shape.block_id);



//				//				var alpha = 1.00f - Maths.Clamp(dist * 0.50f, 0.00f, 1.00f);


//				//				var color = new Vector4(0, 1, 0, 1);

//				//				//if (block.flags.HasAll(Block.Flags.Rare)) color = new Vector4(0.00f, 0.60f, 1.00f, 1.00f);
//				//				//else if (block.flags.HasAll(Block.Flags.Uncommon)) color = new Vector4(0.00f, 1.00f, 0.00f, 1.00f);
//				//				////else if (block.flags.HasAll(Block.Flags.Common)) color = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
//				//				//else color = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);

//				//				color.W = alpha;
//				//				var color_u32 = ImGui.ColorConvertFloat4ToU32(color); // new(0, 1, 0, alpha));

//				//				draw.AddLine(shape_a, shape_b, color_u32, 2.00f);
//				//			}
//				//		}
//				//	}
//				//}

//			}
//		}
//#endif

//		[IComponent.Data(Net.SendType.Reliable)]
//		public struct Data: IComponent
//		{
//			[Statistics.Info("Damage", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
//			public float damage;

//			[Statistics.Info("Reach", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
//			public float max_distance;

//			[Statistics.Info("Area of Effect", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
//			public float radius;

//			public float cooldown;

//			[Save.Ignore, Net.Ignore] public float next_hit;
//			[Save.Ignore, Net.Ignore] public float last_hit;
//		}

//		public static readonly Sound.Handle[] sounds =
//		{
//			"pickaxe_hit_0",
//			"pickaxe_hit_1",
//			"pickaxe_hit_2",
//			"pickaxe_hit_3",
//			"pickaxe_hit_4"
//		};

//#if CLIENT
//		[ISystem.GUI(ISystem.Mode.Single)]
//		public static unsafe void OnGUI(ISystem.Info info, Entity entity, [Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Pickaxe.Data pickaxe, [Source.Owned] in Transform.Data transform, [Source.Parent] in Player.Data player, [Source.Owned] in Control.Data control, [Source.Parent, Optional] in Specialization.Miner.Data mining)
//		{
//			if (player.IsLocal())
//			{
//				var dir = (control.mouse.position_new - transform.position).GetNormalized(out var len);
//				var hit_position = transform.position + (dir * Maths.Clamp(len, 0.25f, mining.ApplyReach(pickaxe.max_distance)));

//				var gui = new BlockGUI()
//				{
//					entity = entity,
//					transform = transform,
//					pickaxe = pickaxe,
//					world_position = hit_position,
//					mining = mining,
//					valid = true
//				};

//				if (info.GetRegion().TryLinecast(transform.position, hit_position, 0.00f, out var hit, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static))
//				{
//					gui.world_position = hit.world_position;
//					//gui.valid = true;
//				}

//				gui.Submit();
//			}
//		}
//#endif

//#if CLIENT
//		[ISystem.Update(ISystem.Mode.Single)]
//		public static unsafe void OnSpriteUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref Pickaxe.Data pickaxe, [Source.Owned] ref Animated.Renderer.Data renderer)
//		{
//			var elapsed = info.WorldTime - pickaxe.last_hit;
//			var max = pickaxe.next_hit - pickaxe.last_hit;
//			var alpha = 1.00f - Maths.Clamp(elapsed / (max * 0.80f), 0.00f, 1.00f);

//			renderer.offset = new Vector2(1.00f, 1.00f) * alpha * alpha;
//			renderer.rotation = -2.50f * alpha * alpha;
//		}
//#endif

//		[ISystem.Update(ISystem.Mode.Single)]
//		public static unsafe void Update(ISystem.Info info, Entity entity, [Source.Owned] ref Pickaxe.Data pickaxe, [Source.Owned] in Transform.Data transform, [Source.Owned] in Body.Data body, [Source.Owned] in Control.Data control, [Source.Parent, Optional] in Specialization.Miner.Data miner)
//		{
//			if (control.mouse.GetKey(Mouse.Key.Left) && info.WorldTime >= pickaxe.next_hit)
//			{
//				//App.WriteLine(mining.level_speed);

//				ref var region = ref info.GetRegion();
//				pickaxe.last_hit = info.WorldTime;
//				pickaxe.next_hit = info.WorldTime + miner.ApplySpeed(pickaxe.cooldown);

//#if SERVER
//				var dir = (control.mouse.position_new - transform.position).GetNormalized(out var len);
//				var normal = -dir;
//				var material_type = default(Material.Type);

//				var hit_position = transform.position + (dir * Maths.Clamp(len, 0.25f, miner.ApplyReach(pickaxe.max_distance)));

//				if (region.TryLinecast(transform.position, hit_position, 0.00f, out var hit, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static))
//				{
//					hit_position = hit.world_position;
//					normal = hit.normal;
//					material_type = (Material.Type)hit.info.shape->material_type;
//				}
//				var random = XorRandom.New();

//				if (material_type == 0)
//				{
//					if (Terrain.TryGetTileAtWorldPosition(region.terrain, hit_position, out var tile))
//					{
//						material_type = (Material.Type)tile.Block.material_type;
//					}
//				}

//				if (material_type != 0)
//				{
//					var parent = body.GetParent();
//					Damage.Hit(entity, parent, default, hit_position, dir, normal, miner.ApplyPower(pickaxe.damage), material_type, Damage.Type.Pickaxe, yield: 1.00f, size: miner.ApplySize(1.00f));
//				}
//#endif
//			}
//		}
//	}
//}

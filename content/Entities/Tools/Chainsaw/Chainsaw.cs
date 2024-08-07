﻿
namespace TC2.Base.Components
{
	public static partial class Chainsaw
	{
		public static readonly Texture.Handle texture_stone = "StoneGib";
		public static readonly Texture.Handle texture_smoke = "LargeSmoke";

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Damage", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float damage;

			[Statistics.Info("Reach", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float max_distance;

			[Statistics.Info("Speed", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.High)]
			public float speed;

			[Statistics.Info("Area of Effect", description: "TODO: Desc", format: "{0:0.##}", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float radius;

			[Save.Ignore, Net.Ignore] public float last_hit;
			[Save.Ignore, Net.Ignore] public float next_hit;
		}

		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.50f)]
		public static void UpdateHoldable([Source.Owned] in Chainsaw.Data chainsaw, [Source.Owned] ref Holdable.Data holdable)
		{
			holdable.hints.AddFlag(NPC.ItemHints.Melee | NPC.ItemHints.Dangerous | NPC.ItemHints.Weapon | NPC.ItemHints.Short_Range | NPC.ItemHints.Usable);
		}

#if CLIENT
		public struct ChainsawGUI: IGUICommand
		{
			public Transform.Data transform;
			public Vector2 world_position;
			public Chainsaw.Data chainsaw;
			public Entity entity;
			//public Specialization.Miner.Data mining;
			public bool valid;

			public void Draw()
			{
				ref var region = ref this.entity.GetRegion();

				var wpos = this.world_position;
				var cpos = region.WorldToCanvas(wpos);

				var radius = this.chainsaw.radius;

				var color = this.valid ? new Color32BGRA(0xff00ff00) : new Color32BGRA(0xffff0000);
				var color_bg = color.WithAlphaMult(0.10f);
				var color_fg = color.WithAlphaMult(0.40f);

				Span<OverlapResult> hits = stackalloc OverlapResult[32];
				if (region.TryOverlapPointAll(wpos, radius * 0.50f, ref hits, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static))
				{
					// TODO: add public API for drawing raw lines in GUI
					foreach (ref var hit in hits)
					{
						//var shape = hit.info.shape;
						//if (shape->klass->type == Chipmunk2D.cpShapeType.CP_SEGMENT_SHAPE)
						//{
						//	var segment = (cpSegmentShape*)shape;

						//	var shape_a = this.WorldToCanvas(segment->ta);
						//	var shape_b = this.WorldToCanvas(segment->tb);

						//	var dist = hit.distance;

						//	ref var block = ref Block.registry[segment->shape.block_id];
						//	var alpha = 1.00f - Maths.Clamp(dist * 0.50f, 0.00f, 1.00f);
						//	var color = new Vector4(0, 1, 0, 1);

						//	color.W = alpha;
						//	var color_u32 = ImGui.ColorConvertFloat4ToU32(color); // new(0, 1, 0, alpha));

						//	draw.AddLine(shape_a, shape_b, color_u32, 2.00f);
						//}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnGUI(ISystem.Info info, ref Region.Data region, Entity entity, [Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Chainsaw.Data chainsaw, [Source.Owned] in Transform.Data transform, [Source.Parent] in Player.Data player, [Source.Owned] in Control.Data control)
		{
			var dir = transform.GetDirection();
			var len = (control.mouse.position - transform.position).Length();
			var hit_position = transform.position + (dir * Maths.Clamp(len, 0.25f, chainsaw.max_distance));

			var gui = new ChainsawGUI()
			{
				entity = entity,
				transform = transform,
				chainsaw = chainsaw,
				world_position = hit_position,
				valid = true
			};

			if (region.TryLinecast(transform.position, hit_position, chainsaw.radius, out var hit, mask: Physics.Layer.World, query_flags: Physics.QueryFlag.Static))
			{
				gui.world_position = hit.world_position;
			}

			gui.Submit();
		}
#endif

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] ref Chainsaw.Data chainsaw, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body,
		[Source.Owned] ref Sound.Emitter sound_emitter, [Source.Owned] ref Animated.Renderer.Data renderer,
		[Source.Owned, Optional(true)] ref Heat.Data heat, [Source.Owned, Optional(true)] ref Heat.State heat_state, 
		[Source.Parent, Optional] in Faction.Data faction)
		{
			if (control.mouse.GetKey(Mouse.Key.Left) && (heat_state.IsNull() || heat_state.flags.HasNone(Heat.State.Flags.Overheated)))
			{
				if (info.WorldTime >= chainsaw.next_hit)
				{
					chainsaw.next_hit = info.WorldTime + MathF.ReciprocalEstimate(chainsaw.speed);

					var max_distance = chainsaw.max_distance;

					var dir = (control.mouse.position - transform.position).GetNormalized(out var len);
					len = Maths.Min(len, max_distance);

					var modifier = 1.00f;

					Span<LinecastResult> hits = stackalloc LinecastResult[16];
					if (region.TryLinecastAll(transform.position + (dir * len * 0.25f), transform.position + (dir * len), 0.000f, ref hits, mask: Physics.Layer.World | Physics.Layer.Destructible))
					{
						var parent = body.GetParent();

						var damage_base = chainsaw.damage;
						var damage_bonus = 0.00f; // random.NextFloatRange(0.00f, melee.damage_bonus);
						var damage = damage_base + damage_bonus;

#if SERVER
						var flags = Damage.Flags.None;
#endif
						var penetration = 0;

						var hit_terrain = false;

						for (var i = 0; i < hits.Length && penetration >= 0; i++)
						{
							ref var hit = ref hits[i];
							if (hit.entity == parent || hit.entity_parent == parent || hit.entity == entity) continue;
							var is_terrain = !hit.entity.IsValid();

							if (is_terrain)
							{
								if (hit_terrain) continue;
								hit_terrain = true;
							}

#if SERVER
							//Damage.Hit(entity, parent, hit.entity, hit.world_position, dir, -dir, damage * modifier, hit.material_type, Damage.Type.Saw, knockback: 1.00f, size: 0.125f, flags: flags, yield: 0.90f, primary_damage_multiplier: 1.00f, secondary_damage_multiplier: 1.00f, terrain_damage_multiplier: 1.00f, faction_id: faction.id, speed: 8.00f);
							Damage.Hit(ent_attacker: entity, ent_owner: parent, ent_target: hit.entity,
								position: hit.world_position, velocity: dir * 8.00f, normal: -dir,
								damage_integrity: damage * modifier, damage_durability: damage * modifier, damage_terrain: damage * modifier,
								target_material_type: hit.material_type, damage_type: Damage.Type.Saw,
								yield: 0.90f, size: 0.125f, impulse: 0.00f, faction_id: faction.id);
#endif

							//flags |= Damage.Flags.No_Sound;
							modifier *= 0.60f;
							penetration--;

							if (heat_state.IsNotNull())
							{
								heat_state.AddEnergy(damage * 0.0009f);
							}
						}

#if CLIENT
						Shake.Emit(ref region, transform.position, 0.25f, 0.25f);
#endif
					}

#if CLIENT
					sound_emitter.volume_mult = Maths.Lerp(sound_emitter.volume_mult, 1.50f, 0.50f);
					sound_emitter.pitch_mult = Maths.Lerp(sound_emitter.pitch_mult, 0.60f + (modifier * 0.80f), 0.20f);
#endif
				}
			}
			else
			{
#if CLIENT
				sound_emitter.volume_mult = Maths.Lerp(sound_emitter.volume_mult, 0.00f, 0.05f);
				sound_emitter.pitch_mult = Maths.Lerp(sound_emitter.pitch_mult, 0.60f, 0.10f);

				renderer.offset = default;
#endif
			}

#if CLIENT
			renderer.offset = random.NextVector2(sound_emitter.volume * sound_emitter.volume_mult * 0.10f);
#endif
		}
	}
}

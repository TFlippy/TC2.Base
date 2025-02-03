
namespace TC2.Base.Components
{
	public static partial class HeadBob
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Asset.Ignore] public Vector2 offset;
			public Vector2 multiplier;
			public float speed;
		}
	}

	public static partial class Head
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region), IComponent.With<Head.State>()]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,


			}

			[Editor.Picker.Position(true, true)]
			public Vector2 offset_mouth;

			public float air_capacity = 0.500f;
			public float air_usage = 0.100f;
			public float breath_interval = 0.800f;
			public float breath_amount = 0.100f;

			public float voice_pitch = 1.00f;

			public Head.Data.Flags flags;
			public byte frame_pain;
			public byte frame_dead;
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct State(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Is_Breathing = 1 << 0,
				Is_Underwater = 1 << 1,
				Is_Suffocating = 1 << 2,
				Is_Holding_Breath = 1 << 3,
			}

			public Head.State.Flags flags;

			[Asset.Ignore] public float concussion;
			//[Save.Ignore, Asset.Ignore] public float air_current;
			[Asset.Ignore] public float air_current_norm = 1.00f;
			[Asset.Ignore] public float air_stored;

			[Save.Ignore, Asset.Ignore, Net.Ignore] public float t_next_breath;
			[Save.Ignore, Asset.Ignore, Net.Ignore] public float t_next_air_check;

			[Save.Ignore, Asset.Ignore, Net.Ignore] public float t_next_sound;
			[Save.Ignore, Asset.Ignore, Net.Ignore] public float t_next_pain;
			[Save.Ignore, Asset.Ignore, Net.Ignore] public float t_next_talk;
		}

#if CLIENT
		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Any)]
		public static void OnGUI(ISystem.Info info, Entity entity, 
		[Source.Owned] in Head.Data head, [Source.Owned] in Head.State head_state)
		{
			var air_current_norm = head_state.air_current_norm;
			var color = Color32BGRA.FromHSV(air_current_norm.Pow2() * 2.00f, 1.00f, 1.00f);

			IStatusEffect.ScheduleDraw(new()
			{
				icon = GUI.tex_icons_widget.GetSprite(16, 16, 5, 2),
				value = $"Air\n{air_current_norm:P0}",
				text_color = color,
				name = $"Air\n{air_current_norm:P0}"
			});
		}
#endif

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateAir(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] in Head.Data head, [Source.Owned] ref Head.State head_state,
		[Source.Owned] in Body.Data body, [Source.Owned] in Transform.Data transform,
		[Source.Owned, Override] ref Organic.Data organic_override, [Source.Owned] ref Organic.State organic_state)
		{
			var time = info.WorldTime;
			head_state.air_stored.MoveTowards(0.00f, head.air_usage * info.DeltaTime);
			if (time >= head_state.t_next_air_check)
			{
				head_state.t_next_air_check = time + 0.50f;
				var is_underwater = false;

				if (body.HasArbiters())
				{
					foreach (var arbiter in body.GetArbiters())
					{
						var layer = arbiter.GetLayer();
						if (layer.HasAny(Physics.Layer.Water))
						{
#if CLIENT
							//region.DrawDebugRect(arbiter.GetAABB(), Color32BGRA.Green);
#endif

							is_underwater = arbiter.ContainsPointAABB(transform.LocalToWorld(head.offset_mouth) - new Vector2(0.00f, 0.50f));
							break;
						}
					}
				}

				head_state.flags.SetFlag(Head.State.Flags.Is_Underwater, is_underwater);
			}

			head_state.flags.SetFlag(Head.State.Flags.Is_Holding_Breath, head_state.flags.HasAny(Head.State.Flags.Is_Underwater));
			if (time >= head_state.t_next_breath && head_state.flags.HasNone(Head.State.Flags.Is_Holding_Breath))
			{
				if (head_state.t_next_breath == 0.00f) head_state.air_stored = head.air_capacity;

				head_state.t_next_breath = time + head.breath_interval;
				head_state.air_stored += head.breath_amount;
				head_state.air_stored.ClampMaxRef(head.air_capacity * 1.50f);
			}

			head_state.air_current_norm = Maths.Avg(head_state.air_current_norm, Maths.Normalize01Fast(head_state.air_stored, head.air_capacity));
			organic_override.consciousness *= head_state.air_current_norm;
		}

		[ISystem.Event<Emote.EmoteEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnEmote(ISystem.Info info, Entity entity, ref XorRandom random, ref Emote.EmoteEvent ev, 
		[Source.Owned] ref Emote.Data emote, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Head.Data head)
		{
			ref var emote_data = ref ev.h_emote.GetData();
			if (emote_data.IsNotNull() && info.WorldTime >= emote.t_last_emote + 0.50f)
			{
				emote.h_emote = ev.h_emote;
				emote.t_last_emote = info.WorldTime;
				emote.t_emote_rem = emote_data.duration;

#if CLIENT
				Sound.Play(emote_data.sound, transform.position, volume: emote_data.volume, pitch: head.voice_pitch * random.NextFloatRange(0.99f, 1.01f) * emote_data.pitch);
#endif
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.20f), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateSprite(ISystem.Info info, 
		[Source.Owned] ref Organic.State organic_state, [Source.Owned] in Head.Data head, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			renderer.sprite.frame.x = organic_state.pain_shared > 200.00f ? head.frame_pain : 0u;
		}

		[ISystem.AddFirst(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", true, Source.Modifier.Owned)]
		public static void OnDeath(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Head.Data head, 
		[Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned, Original] in Organic.Data organic, [HasTag("female", true, Source.Modifier.Owned)] bool is_female)
		{
			renderer.sprite.frame.x = head.frame_dead;

#if SERVER
			ref var species_data = ref organic.h_species.GetData();
			if (species_data.IsNotNull())
			{
				var h_sound = (is_female ? species_data.sounds_death_female : species_data.sounds_death_male).GetRandom(ref random);
				if (h_sound) Sound.Play(ref region, h_sound, transform.LocalToWorld(head.offset_mouth), pitch: random.NextFloatFMA(0.02f, head.voice_pitch));
			}
#endif

			//#if SERVER
			//			WorldNotification.Push(ref region, "* Dies *", 0xffff0000, transform.position, lifetime: 1.00f);
			//#endif
		}

#if SERVER
		[WIP]
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateVoice(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Head.Data head, [Source.Owned] ref Head.State head_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Any, Override] in NPC.Data npc_override,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state)
		{
			ref var species_data = ref organic.h_species.GetData();
			if (species_data.IsNotNull())
			{
				var time = info.WorldTime;

				if (organic_state.unconscious_time > 0.50f)
				{
					head_state.t_next_sound = Maths.Min(head_state.t_next_sound, time + 2.50f);
				}

				var pain_delta = Maths.Max(organic_state.pain_shared, 0.00f);
				if (time >= head_state.t_next_pain && organic_state.consciousness_shared > 0.40f)
				{
					//if (pain_delta >= 800.00f)
					//{
					//	if (random.NextBool(0.90f))
					//	{
					//		Sound.Play(ref region, Kobold.snd_scream.GetRandom(ref random), transform.position, volume: 0.45f, pitch: random.NextFloatRange(0.60f, 0.80f) * head.voice_pitch);
					//		head_state.next_sound = time + random.NextFloatRange(1.50f, 3.00f);
					//	}
					//	giant.next_pain = time + 3.00f;
					//}
					//else if (pain_delta >= 300.00f)
					//{
					//	if (random.NextBool(0.90f))
					//	{
					//		Sound.Play(ref region, snd_oof.GetRandom(ref random), transform.position, volume: 0.45f, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
					//		head_state.next_sound = time + random.NextFloatRange(1.50f, 2.00f);
					//	}
					//	giant.next_pain = time + 1.00f;
					//}
					//else if (organic_state.pain >= 200.00f)
					//{
					//	if (random.NextBool(0.20f))
					//	{
					//		Sound.Play(ref region, snd_pain_slow.GetRandom(ref random), transform.position, volume: 0.45f, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
					//		head_state.next_sound = time + random.NextFloatRange(3.50f, 6.00f);
					//	}
					//	giant.next_pain = time + 3.00f;
					//}

					if (organic_state.pain_shared >= 200.00f)
					{
						if (random.NextBool(0.50f))
						{
							//Sound.Play(ref region, Giant.snd_pain.GetRandom(ref random), transform.position, volume: 0.45f, pitch: random.NextFloatRange(0.80f, 1.00f) * head.voice_pitch);
							head_state.t_next_sound = time + random.NextFloatRange(4.50f, 8.00f);
						}
						head_state.t_next_pain = time + 3.00f;
					}
				}

				if (time >= head_state.t_next_sound)
				{
					if (organic_state.consciousness_shared > 0.10f && (organic_state.unconscious_time > 3.00f || (organic_state.efficiency < 0.50f && organic_state.pain > 50.00f)))
					{
						var lerp = Maths.Normalize01(organic_state.unconscious_time, 10.00f);

						//Sound.Play(ref region, snd_cough.GetRandom(ref random), transform.position, volume: 0.35f * Maths.Lerp(1.00f, 0.50f, lerp), pitch: random.NextFloatRange(0.80f, 1.00f) * Maths.Lerp(1.00f, 0.80f, lerp) * head.voice_pitch);
						head_state.t_next_sound = time + random.NextFloatRange(2.50f, 4.50f + lerp);

						//body.AddImpulse(random.NextUnitVector2Range(50.00f, 150.00f));
					}
					else
					{
						if (organic_state.efficiency < 0.60f)
						{
							if (organic_state.pain > 2000.00f)
							{

							}
						}
						else
						{
							if (time >= head_state.t_next_talk)
							{
								if (random.NextBool(0.70f))
								{
									//Sound.Play(ref region, Giant.snd_cough.GetRandom(ref random), transform.position, volume: 0.40f, pitch: random.NextFloatRange(0.90f, 1.05f) * head.voice_pitch);
								}
								else
								{
									//Sound.Play(ref region, Giant.snd_laugh.GetRandom(ref random), transform.position, volume: 0.50f, pitch: random.NextFloatRange(0.90f, 1.05f) * head.voice_pitch);
								}

								//var text = sb.ToString().Trim();

								//speech_bubble.text = text;
								//speech_bubble.Sync(ent_speech_bubble);

								//ai_original.anger -= Maths.Min(ai_original.anger, random.NextFloatRange(50.00f, 300.00f));

								head_state.t_next_talk = time + random.NextFloatRange(15.00f, 50.00f);
								head_state.t_next_sound = time + 1.00f;
							}
						}
					}
				}
			}
		}
#endif

#if SERVER
		//[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", true, Source.Modifier.Owned)]
		//public static void OnDeath(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] in Head.Data head)
		//{
		//	Sound.Play(ref info.GetRegion(), head.sound_death, transform.position);
		//}

		//[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		//public static void OnUpdateGroan(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Organic.State organic_state, [Source.Owned] ref Head.Data head, [Source.Owned] in Transform.Data transform)
		//{
		//	//if (info.WorldTime > head.next_sound)
		//	//{
		//	//	//App.WriteLine(organic_state.pain_shared);
		//	//	if (organic_state.pain_shared > 500.00f && organic_state.consciousness_shared > 0.30f)
		//	//	{
		//	//		ref var region = ref info.GetRegion();
		//	//		var random = XorRandom.New();
		//	//		Sound.Play(ref region, head.sound_pain, transform.position, volume: 0.20f + Maths.Clamp(organic_state.pain_shared / 2000.00f, 0.20f, 0.50f), pitch: random.NextFloatRange(0.70f, 1.10f) * (1.00f + Maths.Clamp((organic_state.pain_shared_new / 2000.00f), 0.00f, 0.10f)) * head.voice_pitch);
		//	//		head.next_sound = info.WorldTime + (Maths.Clamp(10.00f - (organic_state.pain_shared / 300.00f), 5.00f, 10.00f) * random.NextFloatRange(0.80f, 1.30f));
		//	//	}
		//	//}
		//}
#endif

		//[ISystem.Event<Health.DamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnPostDamage(ISystem.Info info, ref Health.DamageEvent data, [Source.Owned] in Health.Data health, [Source.Owned] ref Head.Data head, [Source.Owned] in Transform.Data transform)
		//{
		//	//var amount_knockback = data.knockback * 0.0002f;
		//	//var amount_damage = Maths.NormalizeClamp(data.damage_durability, health.GetHealth() * 0.50f);

		//	//var amount = MathF.Abs(Maths.Max(amount_knockback, amount_damage));
		//	var amount = data.stun * 0.10f;
		//	//App.WriteLine($"{amount}; {data.knockback}; {data.damage_durability}; {amount_knockback}; {amount_damage}");

		//	//switch (data.damage_type)
		//	//{
		//	//	case Damage.Type.Life:
		//	//	case Damage.Type.Fire:
		//	//	case Damage.Type.Radiation:
		//	//	case Damage.Type.Scratch:
		//	//	case Damage.Type.Slash:
		//	//	case Damage.Type.Stab:
		//	//	case Damage.Type.Bite:
		//	//	case Damage.Type.Electricity:
		//	//	case Damage.Type.Sting:
		//	//	{
		//	//		amount *= 0.10f;
		//	//	}
		//	//	break;

		//	//	case Damage.Type.Impact:
		//	//	case Damage.Type.Blunt:
		//	//	case Damage.Type.Club:
		//	//	case Damage.Type.Ram:
		//	//	case Damage.Type.Crush:
		//	//	{
		//	//		amount *= 1.20f;
		//	//	}
		//	//	break;

		//	//	case Damage.Type.Shockwave:
		//	//	case Damage.Type.Explosion:
		//	//	{
		//	//		amount *= 5.00f;
		//	//	}
		//	//	break;

		//	//	default:
		//	//	{
		//	//		amount *= 1.00f;
		//	//	}
		//	//	break;
		//	//}

		//	//App.WriteLine(amount, App.Color.Cyan);

		//	head.concussion = Maths.Clamp(head.concussion + amount, 0.00f, 1.50f);
		//}

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, 
		[Source.Owned] ref Head.Data head, [Source.Owned] ref Head.State head_state, [Source.Owned] in Organic.State organic_state)
		{
			//App.WriteLine(organic_state.stun_norm);
			head_state.concussion = Maths.Max(Maths.MoveTowards(head_state.concussion, 0.00f, info.DeltaTime * 0.15f), organic_state.stun_norm);
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateNoRotate(ISystem.Info info, 
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, 
		[Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Owned] in Head.Data head)
		{
			no_rotate.multiplier *= MathF.Round(organic_state.consciousness_shared * organic_state.efficiency * Maths.Lerp(0.20f, 1.00f, organic.motorics * organic.motorics)) * organic.coordination * organic.motorics * (1.00f - organic_state.stun_norm);
			no_rotate.speed *= Maths.Lerp01(0.90f, 1.00f, organic.motorics);
			no_rotate.bias += (1.00f - organic.motorics.Clamp01()) * 0.05f;
		}

		// TODO: this causes a weird crash in Flecs
		//[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnRemoveHead([Source.Parent, Override] ref Organic.Data organic, [Source.Parent] ref Organic.State organic_state, [Source.Owned] in Head.Data head, [Source.Parent] in Joint.Base joint)
		//{
		//	if (joint.flags.HasAll(Joint.Flags.Organic))
		//	{
		//		organic_state.consciousness_shared = 0.00f;
		//		organic_state.motorics_shared = 0.00f;
		//	}
		//}

#if SERVER
		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasComponent<Player.Data>(Source.Modifier.Parent, false), HasComponent<NPC.Data>(Source.Modifier.Parent, true)]
		public static void OnUpdateNPC(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Head.Data head, [Source.Owned] ref Head.State head_state, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Transform.Data transform, 
		[Source.Parent] ref Control.Data control, [Source.Parent, Pair.Component<Control.Data>, Optional(true)] ref Net.Synchronized sync)
		{
			if (head_state.concussion > 0.01f && (sync.IsNull() || sync.player_id == 0))
			{
				var offset = new Vector2(0.50f - Maths.Perlin(info.WorldTime, 0.00f, 3.00f, seed: entity.GetShortID()), 0.50f - Maths.Perlin(0.00f, info.WorldTime, 3.00f, seed: entity.GetShortID())) * 8 * head_state.concussion.ClampX1();

				//region.DrawDebugCircle(control.mouse.position, 0.125f, Color32BGRA.Yellow, 4);


				control.mouse.position = Vector2.Lerp(control.mouse.position, transform.position + offset, Maths.Clamp01(head_state.concussion * 1.50f));
				//control.mouse.position += offset * head.concussion;

				//region.DrawDebugCircle(control.mouse.position, 0.25f, Color32BGRA.Red, 4);
			}
		}
#endif

#if CLIENT
		[ISingleton.Data(persist: false), IComponent.With<Sound.Emitter>()]
		public struct Singleton(): ISingleton
		{
			public float tinnitus_volume = 0.00f;
		}

		public static Sound.Handle sound_tinnitus = "tinnitus.loop.00";

		[ISystem.AddFirst(ISystem.Mode.Single, ISystem.Scope.Region, defer: false)]
		public static void OnAddGlobalSoundEmitter(ISystem.Info info, Entity entity, 
		[Source.Owned] ref Head.Singleton head_global, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			//App.WriteLine("add", App.Color.Magenta);

			sound_emitter.file = sound_tinnitus;
			sound_emitter.channel_type = Sound.ChannelType.Master;
			//sound_emitter.flags |= Sound.Emitter.Flags.No_DSP;
			sound_emitter.volume = 1.00f;
			sound_emitter.volume_mult = 0.00f;
			sound_emitter.pitch = 1.50f;
			sound_emitter.mix_3d = 1.00f;
			sound_emitter.priority = 0.60f;
			sound_emitter.spread = 150.00f;
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateGlobalSound(ISystem.Info info, Entity entity, 
		[Source.Owned] ref Head.Singleton head_global, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			//sound_emitter.pitch = 1.50f;
			sound_emitter.volume_mult = Maths.Lerp2(sound_emitter.volume_mult, Maths.Clamp(head_global.tinnitus_volume, 0.00f, 0.10f), 0.05f, 0.40f);

			//if (!sound_emitter.flags.HasAny(Sound.Emitter.Flags.No_DSP))
			//{
			//	sound_emitter.flags |= Sound.Emitter.Flags.No_DSP;
			//	sound_emitter.Refresh();
			//}

			//App.WriteLine(sound_emitter.volume);


			head_global.tinnitus_volume = 0.00f;
		}

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnUpdateConcussionEffects(ISystem.Info info, ref XorRandom random, 
		[Source.Owned] in Head.Data head, [Source.Owned] in Head.State head_state,
		[Source.Singleton] ref Head.Singleton head_global, [Source.Singleton] ref Camera.Singleton camera)
		{
			var modifier = Maths.Clamp01(head_state.concussion);

			head_global.tinnitus_volume = Maths.Lerp01(0.00f, 0.07f, modifier * modifier);

			Drunk.Color.a = Drunk.Color.a.ClampMin(Maths.Lerp01(0.00f, 0.95f, modifier * 1.50f));

			ref var low_pass = ref Audio.LowPass;
			low_pass.frequency = Maths.Lerp01(low_pass.frequency, 800.00f, MathF.Pow(Maths.Max(modifier * 1.80f, 0.00f), 0.50f));
			low_pass.resonance = Maths.Lerp01(low_pass.resonance, 1.50f, Maths.Max(MathF.Pow(modifier * 4.50f, 0.70f), 0.707f));

			camera.rotation = random.NextFloatRange(-0.10f, 0.10f) * Maths.Lerp01(0.00f, 0.50f, modifier * modifier);
		}
#endif

#if CLIENT
		//[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Parent)]
		//public static void OnGUIParent(ISystem.Info info, Entity entity, [Source.Parent] in Player.Data player, [Source.Owned] in Health.Data health, [Source.Owned, Override] in Organic.Data organic)
		//{
		//	IStatusEffect.ScheduleDraw(new()
		//	{
		//		icon = "ui_icon_effect.burning",
		//		//icon_extra = "beer",
		//		value = $"{entity.GetName()}\n{health.integrity:0.00}",
		//		text_color = GUI.font_color_default,
		//		name = $""
		//	});
		//}

		//[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("local", true, Source.Modifier.Shared)]
		//public static void OnGUIShared(ISystem.Info info, Entity entity, [Source.Shared] in Player.Data player, [Source.Owned] in Health.Data health, [Source.Owned, Override] in Organic.Data organic)
		//	=> OnGUIParent(info, entity, in player, in health, in organic);

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateOffset(ISystem.Info info, 
		[Source.Parent] in HeadBob.Data headbob, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			renderer.offset = headbob.offset;
		}

		//[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateOffsetTrait(ISystem.Info info, [Source.Parent] in HeadBob.Data headbob, [Source.Owned, Pair.All] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		//{
		//	//App.WriteLine($"{info.WorldTime}");
		//	renderer.offset = headbob.offset;
		//}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateOffsetHair(ISystem.Info info, 
		[Source.Parent] in HeadBob.Data headbob, [Source.Owned, Pair.Tag("hair")] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			//App.WriteLine($"{info.WorldTime}");
			renderer.offset = headbob.offset;
		}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateOffsetBeard(ISystem.Info info, 
		[Source.Parent] in HeadBob.Data headbob, [Source.Owned, Pair.Tag("beard")] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			//App.WriteLine($"{info.WorldTime}");
			renderer.offset = headbob.offset;
		}
#endif
	}
}

namespace TC2.Base.Components
{
	public static partial class HeadBob
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			[Asset.Ignore] public Vector2 offset;
			public Vector2 multiplier;
			public float speed;
		}
	}

	public static partial class Head
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public float voice_pitch = 1.00f;
			[Asset.Ignore, Save.Ignore] public float concussion;

			public Sound.Handle sound_death;
			public Sound.Handle sound_hit;
			public Sound.Handle sound_pain;
			public Sound.Handle sound_attack;

			[Editor.Picker.Position(true, true)]
			public Vector2 offset_mouth;

			public byte frame_pain;
			public byte frame_dead;

			[Save.Ignore, Net.Ignore] public float next_sound;

			public Data()
			{

			}
		}

		[ISystem.Event<Emote.EmoteEvent>(ISystem.Mode.Single)]
		public static void OnEmote(ISystem.Info info, Entity entity, ref XorRandom random, ref Emote.EmoteEvent data, [Source.Owned] ref Emote.Data emote, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Head.Data head)
		{
			if (data.emote_index < Emote.emotes.Length && info.WorldTime >= emote.last_emote_time + 0.50f)
			{
				ref var emote_data = ref Emote.emotes[data.emote_index];

				emote.emote_index = data.emote_index;
				emote.last_emote_time = info.WorldTime;

#if CLIENT
				Sound.Play(emote_data.sound, transform.position, volume: 0.80f, pitch: head.voice_pitch * random.NextFloatRange(0.99f, 1.01f));
#endif
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, interval: 0.20f), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateSprite(ISystem.Info info, [Source.Owned] ref Organic.State organic_state, [Source.Owned] in Head.Data head, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			renderer.sprite.frame.X = organic_state.pain_shared > 200.00f ? head.frame_pain : 0u;
		}

		[ISystem.AddFirst(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned)]
		public static void OnDeath(ISystem.Info info, ref Region.Data region, [Source.Owned] in Transform.Data transform, [Source.Owned] in Head.Data head, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			renderer.sprite.frame.X = head.frame_dead;

//#if SERVER
//			WorldNotification.Push(ref region, "* Dies *", 0xffff0000, transform.position, lifetime: 1.00f);
//#endif
		}

#if SERVER
		//[ISystem.Add(ISystem.Mode.Single), HasTag("dead", true, Source.Modifier.Owned)]
		//public static void OnDeath(ISystem.Info info, [Source.Owned] in Transform.Data transform, [Source.Owned] in Head.Data head)
		//{
		//	Sound.Play(ref info.GetRegion(), head.sound_death, transform.position);
		//}

		//[ISystem.VeryLateUpdate(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
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

		//[ISystem.Event<Health.DamageEvent>(ISystem.Mode.Single)]
		//public static void OnPostDamage(ISystem.Info info, ref Health.DamageEvent data, [Source.Owned] in Health.Data health, [Source.Owned] ref Head.Data head, [Source.Owned] in Transform.Data transform)
		//{
		//	//var amount_knockback = data.knockback * 0.0002f;
		//	//var amount_damage = Maths.NormalizeClamp(data.damage_durability, health.GetHealth() * 0.50f);

		//	//var amount = MathF.Abs(MathF.Max(amount_knockback, amount_damage));
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

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, [Source.Owned] ref Head.Data head, [Source.Owned] in Organic.State organic_state)
		{
			//App.WriteLine(organic_state.stun_norm);
			head.concussion = MathF.Max(Maths.MoveTowards(head.concussion, 0.00f, info.DeltaTime * 0.15f), organic_state.stun_norm);
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateNoRotate(ISystem.Info info, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state, [Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Owned] in Head.Data head)
		{
			no_rotate.multiplier *= MathF.Round(organic_state.consciousness_shared * organic_state.efficiency * Maths.Lerp(0.20f, 1.00f, organic.motorics * organic.motorics)) * organic.coordination * organic.motorics * (1.00f - organic_state.stun_norm);
			no_rotate.speed *= Maths.Lerp01(0.90f, 1.00f, organic.motorics);
			no_rotate.bias += (1.00f - organic.motorics.Clamp01()) * 0.05f;
		}

		// TODO: this causes a weird crash in Flecs
		//[ISystem.RemoveLast(ISystem.Mode.Single)]
		//public static void OnRemoveHead([Source.Parent, Override] ref Organic.Data organic, [Source.Parent] ref Organic.State organic_state, [Source.Owned] in Head.Data head, [Source.Parent] in Joint.Base joint)
		//{
		//	if (joint.flags.HasAll(Joint.Flags.Organic))
		//	{
		//		organic_state.consciousness_shared = 0.00f;
		//		organic_state.motorics_shared = 0.00f;
		//	}
		//}

#if SERVER
		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single), HasComponent<Player.Data>(Source.Modifier.Parent, false), HasComponent<NPC.Data>(Source.Modifier.Parent, true)]
		public static void OnUpdateNPC(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Head.Data head, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Transform.Data transform, 
		[Source.Parent] ref Control.Data control, [Source.Parent, Pair.Of<Control.Data>, Optional(true)] ref Net.Synchronized sync)
		{
			if (head.concussion > 0.01f && (sync.IsNull() || sync.player_id == 0))
			{
				var offset = new Vector2(0.50f - Maths.Perlin(info.WorldTime, 0.00f, 3.00f, seed: entity.GetShortID()), 0.50f - Maths.Perlin(0.00f, info.WorldTime, 3.00f, seed: entity.GetShortID())) * 8 * head.concussion.ClampX1();

				//region.DrawDebugCircle(control.mouse.position, 0.125f, Color32BGRA.Yellow, 4);


				control.mouse.position = Vector2.Lerp(control.mouse.position, transform.position + offset, Maths.Clamp01(head.concussion * 1.50f));
				//control.mouse.position += offset * head.concussion;

				//region.DrawDebugCircle(control.mouse.position, 0.25f, Color32BGRA.Red, 4);
			}
		}
#endif

#if CLIENT
		[IGlobal.Data(persist: false), IComponent.With<Sound.Emitter>()]
		public struct Global: IGlobal
		{
			public float tinnitus_volume = 0.00f;

			public Global()
			{

			}
		}

		public static Sound.Handle sound_tinnitus = "tinnitus.loop.00";

		[ISystem.AddFirst(ISystem.Mode.Single, defer: false)]
		public static void OnAddGlobalSoundEmitter(ISystem.Info info, Entity entity, [Source.Owned] ref Head.Global head_global, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			//App.WriteLine("add", App.Color.Magenta);

			sound_emitter.file = sound_tinnitus;
			sound_emitter.channel_type = Sound.ChannelType.Master;
			//sound_emitter.flags |= Sound.Emitter.Flags.No_DSP;
			sound_emitter.pitch = 1.50f;
			sound_emitter.mix_3d = 1.00f;
			sound_emitter.priority = 0.60f;
			sound_emitter.spread = 150.00f;
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void OnUpdateGlobalSound(ISystem.Info info, Entity entity, [Source.Owned] ref Head.Global head_global, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			//sound_emitter.pitch = 1.50f;
			sound_emitter.volume = Maths.Lerp2(sound_emitter.volume, Maths.Clamp(head_global.tinnitus_volume, 0.00f, 0.10f), 0.05f, 0.40f);

			//if (!sound_emitter.flags.HasAny(Sound.Emitter.Flags.No_DSP))
			//{
			//	sound_emitter.flags |= Sound.Emitter.Flags.No_DSP;
			//	sound_emitter.Refresh();
			//}

			//App.WriteLine(sound_emitter.volume);


			head_global.tinnitus_volume = 0.00f;
		}

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Parent)]
		public static void OnUpdateConcussionEffects(ISystem.Info info, ref XorRandom random, [Source.Owned] in Head.Data head, [Source.Global] ref Head.Global head_global, [Source.Global] ref Camera.Global camera)
		{
			var modifier = Maths.Clamp01(head.concussion);

			head_global.tinnitus_volume = Maths.Lerp01(0.00f, 0.07f, modifier * modifier);

			Drunk.Color.W = Drunk.Color.W.Max(Maths.Lerp01(0.00f, 0.95f, modifier * 1.50f));

			ref var low_pass = ref Audio.LowPass;
			low_pass.frequency = Maths.Lerp01(low_pass.frequency, 800.00f, MathF.Pow(MathF.Max(modifier * 1.80f, 0.00f), 0.50f));
			low_pass.resonance = Maths.Lerp01(low_pass.resonance, 1.50f, MathF.Max(MathF.Pow(modifier * 4.50f, 0.70f), 0.707f));

			camera.rotation = random.NextFloatRange(-0.10f, 0.10f) * Maths.Lerp01(0.00f, 0.50f, modifier * modifier);
		}
#endif

#if CLIENT
		//[ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Parent)]
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

		//[ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Shared)]
		//public static void OnGUIShared(ISystem.Info info, Entity entity, [Source.Shared] in Player.Data player, [Source.Owned] in Health.Data health, [Source.Owned, Override] in Organic.Data organic)
		//	=> OnGUIParent(info, entity, in player, in health, in organic);

		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnUpdateOffset(ISystem.Info info, [Source.Parent] in HeadBob.Data headbob, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			renderer.offset = headbob.offset;
		}

		//[ISystem.Update(ISystem.Mode.Single)]
		//public static void UpdateOffsetTrait(ISystem.Info info, [Source.Parent] in HeadBob.Data headbob, [Source.Owned, Pair.All] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		//{
		//	//App.WriteLine($"{info.WorldTime}");
		//	renderer.offset = headbob.offset;
		//}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnUpdateOffsetHair(ISystem.Info info, [Source.Parent] in HeadBob.Data headbob, [Source.Owned, Pair.Tag("hair")] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			//App.WriteLine($"{info.WorldTime}");
			renderer.offset = headbob.offset;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnUpdateOffsetBeard(ISystem.Info info, [Source.Parent] in HeadBob.Data headbob, [Source.Owned, Pair.Tag("beard")] ref Animated.Renderer.Data renderer, [Source.Owned] in Head.Data head)
		{
			//App.WriteLine($"{info.WorldTime}");
			renderer.offset = headbob.offset;
		}
#endif
	}
}
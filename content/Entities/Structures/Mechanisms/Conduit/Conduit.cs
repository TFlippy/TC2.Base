
namespace TC2.Base.Components
{
	public static partial class Conduit
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

		}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent/*, ILink*/
		{
			public Conduit.Flags flags;

			//public readonly Entity EntityA => this.a.entity;
			//public readonly Entity EntityB => this.b.entity;
			//public readonly ulong ComponentA => this.a.ComponentID;
			//public readonly ulong ComponentB => this.b.ComponentID;

			public float scale_multiplier = 1.00f;
			public float length;
			public Volume volume;
			public Area cross_section = Area.Circle(10.00f.cm());

			//public EntRef<Air.Vent.Data> a;
			//public EntRef<Air.Vent.Data> b;
		}

#if CLIENT
			[ISystem.PreRender(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void UpdateResizable(ISystem.Info info, Entity entity,
			[Source.Owned] ref Conduit.Data conduit,
			[Source.Owned] ref Resizable.Data resizable, [Source.Owned] ref Transform.Data transform)
			{
				//if (resizable.flags.HasAny(Resizable.Flags.Dynamic) && conduit.a.TryGetHandle(out var vent_a) && conduit.b.TryGetHandle(out var vent_b))
				//{
				//	ref var transform_a = ref conduit.a.entity.GetComponent<Transform.Data>();
				//	ref var transform_b = ref conduit.b.entity.GetComponent<Transform.Data>();

				//	if (transform_a.IsNotNull() && transform_b.IsNotNull())
				//	{
				//		var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
				//		var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

				//		resizable.a = transform.WorldToLocal(pos_a);
				//		resizable.b = transform.WorldToLocal(pos_b);

				//		resizable.cap_a_rotation = transform_a.LocalToWorldRotation(vent_a.data.rotation); // transform_a.LocalToWorldDirection(vent_a.data.direction).GetAngleRadiansFast(); //.LocalToWorldRotation(MathF.PI);
				//		resizable.cap_b_rotation = transform_b.LocalToWorldRotation(vent_b.data.rotation);
				//	}
				//}
			}

			//[ISystem.PreRender(ISystem.Mode.Single, ISystem.Scope.Region)]
			//public static void UpdateRenderer(ISystem.Info info, Entity entity,
			//[Source.Owned] ref Conduit.Data conduit,
			//[Source.Owned] ref Rope.Renderer.Data renderer, [Source.Owned] ref Transform.Data transform)
			//{
			//	//if (conduit.a.TryGetHandle(out var vent_a) && conduit.b.TryGetHandle(out var vent_b))
			//	//{
			//	//	ref var transform_a = ref conduit.a.entity.GetComponent<Transform.Data>();
			//	//	ref var transform_b = ref conduit.b.entity.GetComponent<Transform.Data>();

			//	//	if (transform_a.IsNotNull() && transform_b.IsNotNull())
			//	//	{
			//	//		var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
			//	//		var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

			//	//		var delta = (pos_b - pos_a);
			//	//		var dir = delta.GetNormalized(out var len);

			//	//		var dir_a = transform_a.LocalToWorldRotation(vent_a.data.rotation).RadToDir();
			//	//		var dir_b = transform_b.LocalToWorldRotation(vent_b.data.rotation).RadToDir();

			//	//		renderer.p0 = pos_a + (dir_a * 0.250f);
			//	//		renderer.p3 = pos_b + (dir_b * 0.250f);
			//	//		renderer.p1 = pos_a + (dir_a * 2.50f);
			//	//		renderer.p2 = pos_b + (dir_b * 2.50f);
			//	//		renderer.scale = len * conduit.scale_multiplier;
			//	//	}
			//	//}
			//}
#endif


#if SERVER
		[ISystem.Modified.Component<Conduit.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnModified(ISystem.Info info, Entity entity,
		[Source.Owned] ref Conduit.Data conduit,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Resizable.Data resizable)
		{
			//if (conduit.a.TryGetHandle(out var vent_a) && conduit.b.TryGetHandle(out var vent_b))
			//{
			//	vent_a.data.flags.AddFlag(Air.Vent.Data.Flags.Has_Conduit);
			//	vent_b.data.flags.AddFlag(Air.Vent.Data.Flags.Has_Conduit);

			//	ref var transform_a = ref conduit.a.entity.GetComponent<Transform.Data>();
			//	ref var transform_b = ref conduit.b.entity.GetComponent<Transform.Data>();

			//	if (transform_a.IsNotNull() && transform_b.IsNotNull())
			//	{
			//		var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
			//		var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

			//		var delta = (pos_b - pos_a);
			//		var dir = delta.GetNormalizedFast();

			//		//var rect = new AABB(pos_a, pos_b);

			//		//App.WriteLine("test");

			//		var dir_a = transform_a.LocalToWorldRotation(vent_a.data.rotation).RadToDir();
			//		var dir_b = transform_b.LocalToWorldRotation(vent_b.data.rotation).RadToDir();

			//		//renderer.p1 = pos_a + (new Vector2(pos_b.X - pos_a.X, 0.00f));
			//		//renderer.p2 = renderer.p1 + new Vector2(0.00f, 0.00f);

			//		var p1 = pos_a + (dir_a * 2.50f);
			//		var p2 = pos_b + (dir_b * 2.50f);

			//		var length = 0.00f;
			//		length += Vector2.Distance(pos_a, p1);
			//		length += Vector2.Distance(p1, p2);
			//		length += Vector2.Distance(p2, pos_b);

			//		conduit.length = length;
			//		conduit.volume = Volume.Cylinder(conduit.cross_section, length);


			//		transform.position = pos_a; // (pos_a + pos_b) * 0.50f;
			//		transform.Sync(entity);

			//		resizable.a = transform.WorldToLocal(pos_a);
			//		resizable.b = transform.WorldToLocal(pos_b);
			//		resizable.Modified(entity, true);

			//		//var shape_ptr = entity.GetTraitUnsafe<Body.Data, Shape.Line>();
			//		//if (shape_ptr != null)
			//		//{
			//		//	shape_ptr->a = transform.WorldToLocal(pos_a);
			//		//	shape_ptr->b = transform.WorldToLocal(pos_b);

			//		//	entity.MarkModified<Body.Data, Shape.Line>();

			//		//	entity.SyncTrait<Body.Data, Shape.Line>(shape_ptr);
			//		//}
			//	}

			//	vent_a.Sync();
			//	vent_b.Sync();
			//}
		}

		//[ISystem.Remove(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnRemove(ISystem.Info info, Entity entity,
		//[Source.Owned] ref Conduit.Data conduit)
		//{
		//	//if (conduit.a.TryGetHandle(out var vent_a))
		//	//{
		//	//	vent_a.data.flags.RemoveFlag(Air.Vent.Data.Flags.Has_Conduit);
		//	//	vent_a.Sync();
		//	//}

		//	//if (conduit.b.TryGetHandle(out var vent_b))
		//	//{
		//	//	vent_b.data.flags.RemoveFlag(Air.Vent.Data.Flags.Has_Conduit);
		//	//	vent_b.Sync();
		//	//}
		//}
#endif
	}
}

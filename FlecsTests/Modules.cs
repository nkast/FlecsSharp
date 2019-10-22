﻿using System;
using NUnit.Framework;

namespace Flecs.Tests
{
	[TestFixture]
	public unsafe class Modules : AbstractTest
	{
		struct SimpleModule
		{
			public EntityId PositionEntityId;
			public TypeId PositionTypeId;
			public EntityId VelocityEntityId;
			public TypeId VelocityTypeId;
		}

		void SimpleModuleImport(World world, int flags)
		{
			var handles = ecs.ECS_MODULE<SimpleModule>(world);

			var posTypeId = ecs.ECS_COMPONENT<Position>(world);
			var velTypeId = ecs.ECS_COMPONENT<Velocity>(world);

			handles->PositionEntityId = ecs.type_to_entity(world, posTypeId);
			handles->PositionTypeId = posTypeId;
			handles->VelocityEntityId = ecs.type_to_entity(world, velTypeId);
			handles->VelocityTypeId = velTypeId;
		}

		[Test]
		public void Modules_simple_module()
		{
			ecs.ECS_IMPORT(world, "SimpleModule", SimpleModuleImport, 0);

			var e = ecs.ecs_new<Position>(world);
			Assert.NotZero((UInt64)e);
			Assert.IsTrue(ecs.ecs_has<Position>(world, e));

			ecs.ecs_add<Velocity>(world, e);
			Assert.IsTrue(ecs.ecs_has<Velocity>(world, e));
		}

		void AddVtoP(ref Rows rows)
		{
			var modulePtr = ecs.ecs_column<SimpleModule>(ref rows, 2);

			for (var i = 0; i < rows.count; i++)
				ecs.ecs_add<Velocity>(world, rows[i]);
		}

		[Test]
		public void Modules_import_module_from_system()
		{
			var moduleTypeId = ecs.ECS_IMPORT(world, "SimpleModule", SimpleModuleImport, 0);
			ecs.ECS_SYSTEM(world, AddVtoP, SystemKind.OnUpdate, "Position, $.SimpleModule");

			var module_ptr = ecs.ecs_get_singleton_ptr(world, moduleTypeId);
			Assert.IsTrue(module_ptr != IntPtr.Zero);

			var e = ecs.ecs_new<Position>(world);
			Assert.NotZero((UInt64)e);
			Assert.IsTrue(ecs.ecs_has<Position>(world, e));

			ecs.progress(world, 1);

			Assert.IsTrue(ecs.ecs_has<Velocity>(world, e));
		}
	}
}

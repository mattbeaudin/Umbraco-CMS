﻿using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class UserTypeRepositoryTest : TestWithDatabaseBase
    {
        private UserTypeRepository CreateRepository(IScopeUnitOfWork unitOfWork)
        {
            return new UserTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>());            
        }

        [Test]
        public void Can_Perform_Add_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userType = MockedUserType.CreateUserType();

                // Act
                repository.AddOrUpdate(userType);
                unitOfWork.Flush();

                // Assert
                Assert.That(userType.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Multiple_Adds_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userType1 = MockedUserType.CreateUserType("1");
                var userType2 = MockedUserType.CreateUserType("2");

                // Act
                repository.AddOrUpdate(userType1);
                unitOfWork.Flush();
                repository.AddOrUpdate(userType2);
                unitOfWork.Flush();

                // Assert
                Assert.That(userType1.HasIdentity, Is.True);
                Assert.That(userType2.HasIdentity, Is.True);
            }
        }

        [Test]
        public void Can_Verify_Fresh_Entity_Is_Not_Dirty()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userType = MockedUserType.CreateUserType();
                repository.AddOrUpdate(userType);
                unitOfWork.Flush();

                // Act
                var resolved = repository.Get(userType.Id);
                bool dirty = ((UserType) resolved).IsDirty();

                // Assert
                Assert.That(dirty, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Update_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userType = MockedUserType.CreateUserType();
                repository.AddOrUpdate(userType);
                unitOfWork.Flush();

                // Act
                var resolved = repository.Get(userType.Id);
                resolved.Name = "New Name";
                resolved.Permissions = new[]{"Z", "Y", "X"};
                repository.AddOrUpdate(resolved);
                unitOfWork.Flush();
                var updatedItem = repository.Get(userType.Id);

                // Assert
                Assert.That(updatedItem.Id, Is.EqualTo(resolved.Id));
                Assert.That(updatedItem.Name, Is.EqualTo(resolved.Name));
                Assert.That(updatedItem.Permissions, Is.EqualTo(resolved.Permissions));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userType = MockedUserType.CreateUserType();

                // Act
                repository.AddOrUpdate(userType);
                unitOfWork.Flush();
                var id = userType.Id;

                var repository2 = new UserTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Logger);
                repository2.Delete(userType);
                unitOfWork.Flush();

                var resolved = repository2.Get(id);

                // Assert
                Assert.That(resolved, Is.Null);    
            }
        }

        [Test]
        public void Can_Perform_Get_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userType = MockedUserType.CreateUserType();
                repository.AddOrUpdate(userType);
                unitOfWork.Flush();

                // Act
                var resolved = repository.Get(userType.Id);

                // Assert
                Assert.That(resolved.Id, Is.EqualTo(userType.Id));
                //Assert.That(resolved.CreateDate, Is.GreaterThan(DateTime.MinValue));
                //Assert.That(resolved.UpdateDate, Is.GreaterThan(DateTime.MinValue));
                Assert.That(resolved.Name, Is.EqualTo(userType.Name));
                Assert.That(resolved.Alias, Is.EqualTo(userType.Alias));
                Assert.That(resolved.Permissions, Is.EqualTo(userType.Permissions));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                CreateAndCommitMultipleUserTypes(repository, unitOfWork);

                // Act
                var query = unitOfWork.Query<IUserType>().Where(x => x.Alias == "testUserType1");
                var result = repository.GetByQuery(query);

                // Assert
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_GetAll_By_Param_Ids_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userTypes = CreateAndCommitMultipleUserTypes(repository, unitOfWork);

                // Act
                var result = repository.GetAll(userTypes[0].Id, userTypes[1].Id);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                CreateAndCommitMultipleUserTypes(repository, unitOfWork);

                // Act
                var result = repository.GetAll();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(3));
            }
        }

        [Test]
        public void Can_Perform_Exists_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userTypes = CreateAndCommitMultipleUserTypes(repository, unitOfWork);

                // Act
                var exists = repository.Exists(userTypes[0].Id);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Count_On_UserTypeRepository()
        {
            // Arrange
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repository = CreateRepository(unitOfWork);

                var userTypes = CreateAndCommitMultipleUserTypes(repository, unitOfWork);

                // Act
                var query = unitOfWork.Query<IUserType>().Where(x => x.Alias == "testUserType1" || x.Alias == "testUserType2");
                var result = repository.Count(query);

                // Assert
                Assert.That(result, Is.GreaterThanOrEqualTo(2));
            }
        }

        private IUserType[] CreateAndCommitMultipleUserTypes(IUserTypeRepository repository, IUnitOfWork unitOfWork)
        {
            var userType1 = MockedUserType.CreateUserType("1");
            var userType2 = MockedUserType.CreateUserType("2");
            var userType3 = MockedUserType.CreateUserType("3");
            repository.AddOrUpdate(userType1);
            repository.AddOrUpdate(userType2);
            repository.AddOrUpdate(userType3);
            unitOfWork.Flush();
            return new IUserType[] {userType1, userType2, userType3};
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Infrastructure.Services.MongoDb;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.Infrastructure.Services.MongoDb
{
    public class MongoServicesTests
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _mongoDbSettingsSectionMock;
        private Mock<IConfigurationSection> _connectionStringSectionMock;
        private Mock<IConfigurationSection> _databaseNameSectionMock;
        private Mock<IConfigurationSection> _loanPaymentsDbSectionMock;

        public MongoServicesTests()
        {
            new Mock<IMongoClient>();
            new Mock<IMongoDatabase>();
            new Mock<IMongoCollection<TestDocument>>();
            _configurationMock = new Mock<IConfiguration>();
            _mongoDbSettingsSectionMock = new Mock<IConfigurationSection>();
            _connectionStringSectionMock = new Mock<IConfigurationSection>();
            _databaseNameSectionMock = new Mock<IConfigurationSection>();
            _loanPaymentsDbSectionMock = new Mock<IConfigurationSection>();
        }

        [Fact]
        public void Constructor_WithValidConfiguration_ShouldInitializeClient()
        {
            _connectionStringSectionMock.Setup(s => s.Value).Returns("mongodb://localhost:27017");
            _databaseNameSectionMock.Setup(s => s.Value).Returns("TestDatabase");
            _loanPaymentsDbSectionMock.Setup(s => s.Value).Returns((string)null);

            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("ConnectionString"))
                .Returns(_connectionStringSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("DatabaseName"))
                .Returns(_databaseNameSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("LoanPaymentsDb"))
                .Returns(_loanPaymentsDbSectionMock.Object);

            _configurationMock.Setup(c => c.GetSection("MongoDbSettings"))
                .Returns(_mongoDbSettingsSectionMock.Object);

            var service = new MongoServices(_configurationMock.Object);

            Assert.NotNull(service.Client);
            Assert.NotNull(service.Database);
        }

        [Fact]
        public void Constructor_WithLoanPaymentsDb_ShouldUseLoanPaymentsDb()
        {
            _connectionStringSectionMock.Setup(s => s.Value).Returns("mongodb://localhost:27017");
            _loanPaymentsDbSectionMock.Setup(s => s.Value).Returns("LoanPaymentsDatabase");
            _databaseNameSectionMock.Setup(s => s.Value).Returns("TestDatabase");

            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("ConnectionString"))
                .Returns(_connectionStringSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("DatabaseName"))
                .Returns(_databaseNameSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("LoanPaymentsDb"))
                .Returns(_loanPaymentsDbSectionMock.Object);

            _configurationMock.Setup(c => c.GetSection("MongoDbSettings"))
                .Returns(_mongoDbSettingsSectionMock.Object);

            var service = new MongoServices(_configurationMock.Object);

            Assert.NotNull(service.Database);
        }

        [Fact]
        public void Constructor_WithoutConnectionString_ShouldThrowArgumentNullException()
        {
            _connectionStringSectionMock.Setup(s => s.Value).Returns((string)null);
            _databaseNameSectionMock.Setup(s => s.Value).Returns("TestDatabase");
            _loanPaymentsDbSectionMock.Setup(s => s.Value).Returns((string)null);

            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("ConnectionString"))
                .Returns(_connectionStringSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("DatabaseName"))
                .Returns(_databaseNameSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("LoanPaymentsDb"))
                .Returns(_loanPaymentsDbSectionMock.Object);

            _configurationMock.Setup(c => c.GetSection("MongoDbSettings"))
                .Returns(_mongoDbSettingsSectionMock.Object);

            Assert.Throws<ArgumentNullException>(() => new MongoServices(_configurationMock.Object));
        }

        [Fact]
        public void Constructor_WithoutDatabaseName_ShouldThrowArgumentNullException()
        {
            _connectionStringSectionMock.Setup(s => s.Value).Returns("mongodb://localhost:27017");
            _databaseNameSectionMock.Setup(s => s.Value).Returns((string)null);
            _loanPaymentsDbSectionMock.Setup(s => s.Value).Returns((string)null);

            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("ConnectionString"))
                .Returns(_connectionStringSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("DatabaseName"))
                .Returns(_databaseNameSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("LoanPaymentsDb"))
                .Returns(_loanPaymentsDbSectionMock.Object);

            _configurationMock.Setup(c => c.GetSection("MongoDbSettings"))
                .Returns(_mongoDbSettingsSectionMock.Object);

            Assert.Throws<ArgumentNullException>(() => new MongoServices(_configurationMock.Object));
        }

        [Fact]
        public void GetCollection_WithValidCollectionName_ShouldReturnCollection()
        {
            _connectionStringSectionMock.Setup(s => s.Value).Returns("mongodb://localhost:27017");
            _databaseNameSectionMock.Setup(s => s.Value).Returns("TestDatabase");
            _loanPaymentsDbSectionMock.Setup(s => s.Value).Returns((string)null);

            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("ConnectionString"))
                .Returns(_connectionStringSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("DatabaseName"))
                .Returns(_databaseNameSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("LoanPaymentsDb"))
                .Returns(_loanPaymentsDbSectionMock.Object);

            _configurationMock.Setup(c => c.GetSection("MongoDbSettings"))
                .Returns(_mongoDbSettingsSectionMock.Object);

            var service = new MongoServices(_configurationMock.Object);
            
            var collection = service.GetCollection<TestDocument>("testCollection");

            Assert.NotNull(collection);
        }

        [Fact]
        public void GetCollection_WithCustomDatabase_ShouldReturnCollectionFromCustomDatabase()
        {
            _connectionStringSectionMock.Setup(s => s.Value).Returns("mongodb://localhost:27017");
            _databaseNameSectionMock.Setup(s => s.Value).Returns("TestDatabase");
            _loanPaymentsDbSectionMock.Setup(s => s.Value).Returns((string)null);

            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("ConnectionString"))
                .Returns(_connectionStringSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("DatabaseName"))
                .Returns(_databaseNameSectionMock.Object);
            _mongoDbSettingsSectionMock.Setup(s => s.GetSection("LoanPaymentsDb"))
                .Returns(_loanPaymentsDbSectionMock.Object);

            _configurationMock.Setup(c => c.GetSection("MongoDbSettings"))
                .Returns(_mongoDbSettingsSectionMock.Object);

            var service = new MongoServices(_configurationMock.Object);
            
            var collection = service.GetCollection<TestDocument>("testCollection", "CustomDatabase");

            Assert.NotNull(collection);
        }

        private class TestDocument
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}

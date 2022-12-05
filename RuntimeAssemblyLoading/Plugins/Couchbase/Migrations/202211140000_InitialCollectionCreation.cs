using Couchbase.Management.Collections;
using Couchbase.Management.Query;

using Unibet.Infrastructure.Caching.CouchbaseV7.Migrations;
using Unibet.Infrastructure.Caching.CouchbaseV7.Migrations.Abstractions;

namespace CouchbasePlugin.Migrations;
public class Initial : BaseMigration
{
    public override async Task Up(ICouchbaseEnvironment couchbaseEnvironment)
    {
        var bucketName = "backend-ephemeral";
        var scopeName = couchbaseEnvironment.ScopeName;

        var cluster = couchbaseEnvironment.Cluster;
        var ephemeralBucket = await cluster.BucketAsync(bucketName);
        var scope = await ephemeralBucket.ScopeAsync(scopeName);

        var collectionManager = ephemeralBucket.Collections;

        var collectionNames = new List<string>() { "OddsLadders", "OddsLadderEvents", "Rewards", "cache" };

        foreach (var collectionName in collectionNames)
        {
            await collectionManager.CreateCollectionAsync(new CollectionSpec(scopeName, collectionName));
            //poll-check to wait until the collection is created. This is done so that subsequent operations on the collection
            //dont error out due to collection not found exception. CB works asynchronously. 
            await scope.WaitUntilCollectionIsCreated(collectionName, null, 500, 10);

            var queryIndexManager = cluster.QueryIndexes;

            var primaryIndexName = "#primary";
            await queryIndexManager.CreatePrimaryIndexAsync(
                bucketName,
                new CreatePrimaryQueryIndexOptions()
                    .ScopeName(scopeName)
                    .CollectionName(collectionName)
                    .IgnoreIfExists(true)
                    .Deferred(false)
            );

            await scope.WaitUntilIndexIsOnline(primaryIndexName, collectionName, null, 1000, 20);

        }

    }
}

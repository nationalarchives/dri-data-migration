using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using System.Reflection;

namespace Orchestration.Tests;

[TestClass]
public sealed class EtlTest
{
    public static string DisplayName(MethodInfo _, object[] data) => data[data.Length - 1].ToString()!;

    [TestMethod("ETL method runs successfully")]
    [DynamicData(nameof(RunsData), DynamicDataDisplayName = nameof(DisplayName))]
    public async Task Runs(FakeLogger fakeLogger, IEtl etl, int ingestedEventId, string _)
    {
        await etl.RunAsync("ignore", 0);

        fakeLogger.Collector.LatestRecord.Should().Satisfy<FakeLogRecord>(r => r.Id.Id.Should().Be(ingestedEventId))
            .And.Satisfy<FakeLogRecord>(r => r.StructuredState.Should().ContainSingle(s => s.Value == "1"));
    }

    record SetupInfo<T, T1>(FakeLogger<T> Logger, IDriExporter DriExport, IStagingIngest<T1> Ingest)
        where T : IEtl
        where T1 : IDriRecord;

    public static IEnumerable<object[]> RunsData()
    {
        static SetupInfo<EtlAccessCondition, DriAccessCondition> SetupAc()
        {
            Mock<IDriExporter> driExport = new();
            FakeLogger<EtlAccessCondition> logger = new();
            Mock<IStagingIngest<DriAccessCondition>> ingest = new();
            var dri = new DriAccessCondition(new("http://example.com/access-condition"), "Access condition name");
            driExport.Setup(e => e.GetAccessConditionsAsync()).ReturnsAsync([dri]);
            ingest.Setup(i => i.SetAsync(new List<DriAccessCondition> { dri })).ReturnsAsync(1);

            return new(logger, driExport.Object, ingest.Object);
        }

        static SetupInfo<EtlLegislation, DriLegislation> SetupLeg()
        {
            Mock<IDriExporter> driExport = new();
            FakeLogger<EtlLegislation> logger = new();
            Mock<IStagingIngest<DriLegislation>> ingest = new();
            var dri = new DriLegislation(new("http://example.com/legislation"), "Legislation section");
            driExport.Setup(e => e.GetLegislationsAsync()).ReturnsAsync([dri]);
            ingest.Setup(i => i.SetAsync(new List<DriLegislation> { dri })).ReturnsAsync(1);

            return new(logger, driExport.Object, ingest.Object);
        }

        static SetupInfo<EtlGroundForRetention, DriGroundForRetention> SetupGfr()
        {
            Mock<IDriExporter> driExport = new();
            FakeLogger<EtlGroundForRetention> logger = new();
            Mock<IStagingIngest<DriGroundForRetention>> ingest = new();
            var dri = new DriGroundForRetention("Ground for retention", "GFR description");
            driExport.Setup(e => e.GetGroundsForRetentionAsync()).ReturnsAsync([dri]);
            ingest.Setup(i => i.SetAsync(new List<DriGroundForRetention> { dri })).ReturnsAsync(1);

            return new(logger, driExport.Object, ingest.Object);
        }

        static SetupInfo<EtlSubset, DriSubset> SetupSub()
        {
            Mock<IDriExporter> driExport = new();
            FakeLogger<EtlSubset> logger = new();
            Mock<IStagingIngest<DriSubset>> ingest = new();
            var dri = new DriSubset("Subset", "Subset directory");
            driExport.Setup(e => e.GetSubsetsByCodeAsync("ignore", 0, 0)).ReturnsAsync([dri]);
            ingest.Setup(i => i.SetAsync(new List<DriSubset> { dri })).ReturnsAsync(1);

            return new(logger, driExport.Object, ingest.Object);
        }

        static SetupInfo<EtlAsset, DriAsset> SetupAss()
        {
            Mock<IDriExporter> driExport = new();
            FakeLogger<EtlAsset> logger = new();
            Mock<IStagingIngest<DriAsset>> ingest = new();
            var dri = new DriAsset("Asset", "Asset directory", "Subset");
            driExport.Setup(e => e.GetAssetsByCodeAsync("ignore", 0, 0)).ReturnsAsync([dri]);
            ingest.Setup(i => i.SetAsync(new List<DriAsset> { dri })).ReturnsAsync(1);

            return new(logger, driExport.Object, ingest.Object);
        }

        static SetupInfo<EtlVariation, DriVariation> SetupVar()
        {
            Mock<IDriExporter> driExport = new();
            FakeLogger<EtlVariation> logger = new();
            Mock<IStagingIngest<DriVariation>> ingest = new();
            var dri = new DriVariation(new("http://example.com/variation"), "Variation name", "Asset");
            driExport.Setup(e => e.GetVariationsByCodeAsync("ignore", 0, 0)).ReturnsAsync([dri]);
            ingest.Setup(i => i.SetAsync(new List<DriVariation> { dri })).ReturnsAsync(1);

            return new(logger, driExport.Object, ingest.Object);
        }

        static SetupInfo<EtlSensitivityReview, DriSensitivityReview> SetupSr()
        {
            Mock<IDriExporter> driExport = new();
            FakeLogger<EtlSensitivityReview> logger = new();
            Mock<IStagingIngest<DriSensitivityReview>> ingest = new();
            var dri = new DriSensitivityReview(new("http://example.com/variation"), "Reference",
                new("http://example.com/target"), new("http://example.com/target-type"),
                new("http://example.com/access-condition"), []);
            driExport.Setup(e => e.GetSensitivityReviewsByCodeAsync("ignore", 0, 0)).ReturnsAsync([dri]);
            ingest.Setup(i => i.SetAsync(new List<DriSensitivityReview> { dri })).ReturnsAsync(1);

            return new(logger, driExport.Object, ingest.Object);
        }

        var ac = SetupAc();
        var leg = SetupLeg();
        var gfr = SetupGfr();
        var sub = SetupSub();
        var ass = SetupAss();
        var v = SetupVar();
        var sr = SetupSr();

        return
        [
            [
                ac.Logger,
                new EtlAccessCondition(ac.Logger, ac.DriExport, ac.Ingest),
                8,
                "access condition"
            ],
            [
                leg.Logger,
                new EtlLegislation(leg.Logger, leg.DriExport, leg.Ingest),
                9,
                "legislation"
            ],
            [
                gfr.Logger,
                new EtlGroundForRetention(gfr.Logger, gfr.DriExport, gfr.Ingest),
                10,
                "ground for retention"
            ],
            [
                sub.Logger,
                new EtlSubset(sub.Logger, sub.DriExport, sub.Ingest),
                11,
                "subset"
            ],
            [
                ass.Logger,
                new EtlAsset(ass.Logger, ass.DriExport, ass.Ingest),
                12,
                "asset"
            ],
            [
                v.Logger,
                new EtlVariation(v.Logger, v.DriExport, v.Ingest),
                13,
                "variation"
            ],
            [
                sr.Logger,
                new EtlSensitivityReview(sr.Logger, sr.DriExport, sr.Ingest),
                14,
                "sensitivity review"
            ]
        ];
    }
}

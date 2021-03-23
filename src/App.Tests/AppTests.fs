﻿module AppTests

open HashUtil.Checksum
open HashUtil.Verification
open Xunit
open Xunit.Abstractions

type AppTests(output: ITestOutputHelper) =
    [<Fact>]
    member _.``Console width``() =
        let b = Program.consoleMaxWidth
        Assert.True(b >= 9)
        output.WriteLine("ultimateanu_debug: width-%d", b)

    [<Fact>]
    member _.``RootOpt parses md5 algorithm correctly``() =

        let rootOpt =
            Program.RootOpt([|"report.pdf"; "sources.txt"|],
                true, true, true, false, "md5")

        Assert.Equal(HashType.MD5, rootOpt.Algorithm)
        let expectedStr = "RootOpt[Items:[|\"report.pdf\"; \"sources.txt\"|] PrintTree:true Save:true IncludeHiddenFiles:true SkipEmptyDir:false Algorithm:MD5]"
        Assert.Equal(expectedStr, rootOpt.ToString())

    [<Fact>]
    member _.``RootOpt uses sha1 if algorithm not specified``() =
        let rootOpt =
            Program.RootOpt([|"report.pdf"; "sources.txt"|],
                true, true, true, false, null)

        Assert.Equal(HashType.SHA1, rootOpt.Algorithm)

    [<Fact>]
    member _.``CheckOpt parses algorithm and verbosity correctly``() =
        let checkOpt =
            Program.CheckOpt([|"report.pdf"|], false, true, "sha256", "detailed")

        Assert.True(checkOpt.Algorithm.IsSome)
        Assert.Equal(HashType.SHA256, checkOpt.Algorithm.Value)
        Assert.Equal(PrintVerbosity.Detailed, checkOpt.Verbosity)
        let expectedStr = "CheckOpt[Items:[|\"report.pdf\"|] IncludeHiddenFiles:false SkipEmptyDir:true Algorithm:Some SHA256]"
        Assert.Equal(expectedStr, checkOpt.ToString())

    [<Fact>]
    member _.``CheckOpt sets algorithm None when missing``() =
        let checkOpt =
            Program.CheckOpt([|"report.pdf"|], false, true, null, "detailed")

        Assert.True(checkOpt.Algorithm.IsNone)

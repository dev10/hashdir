open CommandLine
open System.IO
open System.Security.Cryptography
open System.Text


let computeHashString (input : string) =
    input
        |> Encoding.ASCII.GetBytes
        |> SHA256.Create().ComputeHash
        |> Seq.map (fun c -> c.ToString("x2"))
        |> Seq.reduce (+)


let computeHashStringFromFile filePath =
    assert File.Exists filePath
    use file = File.OpenRead filePath
    file
        |> SHA256.Create().ComputeHash
        |> Seq.map (fun c -> c.ToString("x2"))
        |> Seq.reduce (+)


type ItemHash =
    | File of path : string * hash : string
    | Dir of path : string * hash : string * children : seq<ItemHash>


let getHash itemHash =
    match itemHash with
        | File (_, hash) ->
            hash
        | Dir (_, hash, _) ->
            hash


let rec makeDirHashStructure includeHiddenFiles dirPath =
    assert Directory.Exists(dirPath)
    let children =
        dirPath
            |> Directory.EnumerateFileSystemEntries
            |> Seq.sortBy(id)
            |> Seq.choose (makeHashStructure includeHiddenFiles)
    let childrenHash =
        children
            |> Seq.map getHash
            |> Seq.reduce (+)
            |> computeHashString
    Dir(path = dirPath, hash = childrenHash, children = children)


and makeHashStructure includeHiddenFiles path =
    if File.Exists(path) then
        if ((not includeHiddenFiles) &&
            (File.GetAttributes(path) &&& FileAttributes.Hidden).Equals(FileAttributes.Hidden)) then
            None
        else
            Some(File(path = path, hash = (computeHashStringFromFile path)))
    else if Directory.Exists(path) then
        Some(makeDirHashStructure includeHiddenFiles path)
    else
        None

let makeLeftSpacer levels =
    match levels with
        | [] -> ""
        | lastLevelActive :: parentsActive ->
            let parentSpacer =
                parentsActive
                    |> List.rev
                    |> List.map (fun isActive -> if isActive then "│   " else "    ")
                    |> System.String.Concat

            let curSpacer = if lastLevelActive then "├── " else "└── "
            parentSpacer + curSpacer


let rec printHashStructureHelper structure (levels:List<bool>) =
    match structure with
        | File (path, hash) ->
            printfn "%s%s %s" (makeLeftSpacer levels) hash (Path.GetFileName path)
        | Dir (path, hash, children) ->
            printfn "%s%s %c%s" (makeLeftSpacer levels) hash
                Path.DirectorySeparatorChar (DirectoryInfo(path).Name)
            let allButLastChild = Seq.take (Seq.length children - 1) children
            let lastChild = Seq.last children
            for child in allButLastChild do
                printHashStructureHelper child (true :: levels)
            printHashStructureHelper lastChild (false :: levels)


let rec printHashStructure structure =
    printHashStructureHelper structure []


type Options = {
  [<Option('i', "include-hidden-files", Default = false, HelpText = "Include hidden files.")>] IncludeHiddenFiles : bool;
  [<Value(0, Required = true, MetaName="input", HelpText = "Input directories or files.")>] Input : seq<string>;
}


let run (o : Options)  =
    for item in o.Input do
        let optHashStructure = makeHashStructure o.IncludeHiddenFiles item
        match optHashStructure with
            | None -> printfn "%s is not a valid path" item
            | Some(hashStructure) -> printHashStructure hashStructure


[<EntryPoint>]
let main argv =
    let parsedResult = CommandLine.Parser.Default.ParseArguments<Options>(argv)
    match parsedResult with
        | :? Parsed<Options> as parsed -> run parsed.Value
        | _ -> ()

    0
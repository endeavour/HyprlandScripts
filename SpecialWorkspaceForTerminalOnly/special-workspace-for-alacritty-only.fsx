// Monitors new windows being opened and moves them to the previous workspace (except for alacritty)
// Idea being to reserve special workspace for terminal windows only. 

open System
open System.Text.RegularExpressions

// ParseRegex parses a regular expression and returns a list of the strings that match each group in
// the regular expression.
// List.tail is called to eliminate the first element in the list, which is the full matched expression,
// since only the matches for each group are wanted.
let (|ParseRegex|_|) regex str =
   let m = Regex(regex).Match(str)
   if m.Success
   then Some (List.tail [ for x in m.Groups -> x.Value ])
   else None

let (|ParseInt|_|) (str:string) =
  match Int32.TryParse str with
  | true, x -> Some x
  | false, _ -> None

let lines =
  seq {
    while true do
    Console.ReadLine()
    }
  
// TODO: Get initial workspace

type Event =
  | Normal of name:string
  | Special of name:string
  | ClosedSpecial
  | WindowOpened of windowAddr:string * workspaceName:string * windowClass:string * windowTitle:string
  
let isSpecial name =
  name = "special" || name.StartsWith "special:"
  
let parseEvent str =
  match str with
  | ParseRegex "^focusedmon>>.*,(\d+)" [workspaceName] when not <| isSpecial workspaceName -> Some (Normal workspaceName)
  | ParseRegex "^workspacev2>>(\d+),(.*)" [ParseInt workspaceId; workspaceName] when not <| isSpecial workspaceName -> Some (Normal workspaceName)
  | ParseRegex "^focusedmon>>.*,(\d+)" [workspaceName] -> Some (Special workspaceName)
  | ParseRegex "^workspacev2>>(\d+),(.*)" [ParseInt workspaceId; workspaceName]-> Some (Special workspaceName)  
  | ParseRegex "^activespecial>>,(.*)" [monitorName] -> Some(ClosedSpecial)
  | ParseRegex "^activespecial>>(.*),(.*)" [workspaceName; monitorName] -> Some (Special workspaceName)
  | ParseRegex "^openwindow>>(.*),(.*),(.*),(.*)" [windowAddr; workspaceName; windowClass; windowTitle] -> Some (WindowOpened(windowAddr, workspaceName, windowClass, windowTitle))
  | _ -> None
  
let initialWorkspace = "1"  //TODO: pass in as arg?
  
lines
|> Seq.choose parseEvent
|> Seq.scan (fun (lastNormalWorkspace, currentWorkspace, latestEvent) newEvent ->
  match newEvent with
  | Normal workSpaceName -> (workSpaceName, workSpaceName, Some newEvent)
  | Special workSpaceName -> (lastNormalWorkspace, workSpaceName, Some newEvent)
  | ClosedSpecial -> (lastNormalWorkspace, lastNormalWorkspace, Some newEvent)
  | _ -> (lastNormalWorkspace, currentWorkspace, Some newEvent)
  ) (initialWorkspace, initialWorkspace, None)
|> Seq.where (fun (_,currentWorkspace,_) -> isSpecial currentWorkspace)
|> Seq.iter (fun (lastNormalWorkspace, currentWorkspace, evt) ->
  match evt with
  | Some(WindowOpened (windowAddr, workspaceName, windowClass, windowTitle:string)) when windowClass <> "Alacritty" ->
    printfn "dispatch movetoworkspace name:%s,address:0x%s" lastNormalWorkspace windowAddr    
  | _ -> ()
  )    

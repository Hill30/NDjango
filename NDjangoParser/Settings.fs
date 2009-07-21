#light

namespace NDjango

module Settings =
    
    let TEMPLATE_STRING_IF_INVALID = "TEMPLATE_STRING_IF_INVALID"
    let DEFAULT_AUTOESCAPE = "DEFAULT_AUTOESCAPE"
    
    /// adds the key/value pair into the supplied map (usage: map ++ (key, value))
    let private (++) map (key: 'a, value: 'b) = Map.add key value map

    type Setting = 
        | Bool of bool
        | Int of int
        | String of string

    let internal defaultSettings = 
        new Map<string, Setting>([])
            ++ (DEFAULT_AUTOESCAPE, Bool true)
            ++ (TEMPLATE_STRING_IF_INVALID, String "")

//    [<OverloadID("bool")>]
//    let internal apply_setting name (value:bool) settings = ()
//    [<OverloadID("int")>]
//    let internal apply_setting name (value:int) settings = ()
//    [<OverloadID("string")>]
//    let internal apply_setting name (value:string) settings = ()

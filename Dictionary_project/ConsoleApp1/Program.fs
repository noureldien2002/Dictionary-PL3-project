open System
open System.IO
open Newtonsoft.Json
open System.Drawing
open System.Windows.Forms



    member private this.loadDictionary() =
        try
            if File.Exists(dictionaryPath) then
                let json = File.ReadAllText(dictionaryPath)
                dictionary <- JsonConvert.DeserializeObject<Map<string, string>>(json)
                resultBox.Text <- "Dictionary loaded successfully!"
            else
                resultBox.Text <- "No dictionary file found. Starting fresh."
        with
        | ex -> resultBox.Text <- $"Error loading dictionary: {ex.Message}"

    member private this.addOrUpdateWord() =
        let word = wordInput.Text.Trim()
        let definition = definitionInput.Text.Trim()
        if String.IsNullOrEmpty(word) || String.IsNullOrEmpty(definition) then
            resultBox.Text <- "Both word and definition are required to add/update!"
        else
            dictionary <- dictionary.Add(word.ToLowerInvariant(), definition)
            resultBox.Text <- $"Word '{word}' added/updated successfully!"
            wordInput.Clear()
            definitionInput.Clear()

    member private this.deleteWord() =
        let word = wordInput.Text.Trim()
        if String.IsNullOrEmpty(word) then
            resultBox.Text <- "Please provide a word to delete."
        else
            let lowerWord = word.ToLowerInvariant()
            if dictionary.ContainsKey(lowerWord) then
                dictionary <- dictionary.Remove(lowerWord)
                resultBox.Text <- $"Word '{word}' deleted successfully!"
                wordInput.Clear()
            else
                resultBox.Text <- $"Word '{word}' not found in the dictionary."

    member private this.searchWord() =
        let keyword = searchInput.Text.Trim()
        if String.IsNullOrEmpty(keyword) then
            resultBox.Text <- "Please provide a word or keyword to search."
        else
            let lowerKeyword = keyword.ToLowerInvariant()
            match dictionary.TryFind(lowerKeyword) with
            | Some definition -> resultBox.Text <- $"Definition of '{keyword}': {definition}"
            | None ->
                let partialResults = dictionary |> Map.filter (fun key _ -> key.Contains(lowerKeyword))
                if Map.isEmpty partialResults then
                    resultBox.Text <- $"No matches found for '{keyword}'."
                else
                    resultBox.Text <- "Partial Matches:\n" + String.Join("\n", partialResults |> Seq.map (fun kvp -> $"{kvp.Key}: {kvp.Value}"))
            searchInput.Clear()

    member private this.saveToFile() =
        try
            let json = JsonConvert.SerializeObject(dictionary, Formatting.Indented)
            File.WriteAllText(dictionaryPath, json)
            resultBox.Text <- $"Dictionary saved to '{dictionaryPath}'!"
        with
        | ex -> resultBox.Text <- $"Error saving dictionary: {ex.Message}"

[<EntryPoint>]
let main argv =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)
    Application.Run(new MainForm())
    0

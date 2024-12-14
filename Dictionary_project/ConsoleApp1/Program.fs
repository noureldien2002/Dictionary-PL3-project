open System
open System.IO
open Newtonsoft.Json
open System.Drawing
open System.Windows.Forms

type MainForm() as this =
    inherit Form()

    let dictionaryPath = "dictionary.json"
    let mutable dictionary: Map<string, string> = Map.empty

    // gui elements
    let lblWord = new Label(Text = "Word:", Top = 20, Left = 20, Width = 80, Font = new Font("Segoe UI", float32 10, FontStyle.Bold), ForeColor = Color.White)
    let wordInput = new TextBox(Width = 200, Top = 20, Left = 120, Font = new Font("Segoe UI", float32 10))

    let lblDefinition = new Label(Text = "Definition:", Top = 60, Left = 20, Width = 80, Font = new Font("Segoe UI", float32 10, FontStyle.Bold), ForeColor = Color.White)
    let definitionInput = new TextBox(Width = 200, Top = 60, Left = 120, Font = new Font("Segoe UI", float32 10))

    let addButton = new Button(Text = "Add/Update", Top = 100, Left = 20, Width = 140, BackColor = Color.LightGreen, Font = new Font("Segoe UI", float32 10, FontStyle.Bold))
    let deleteButton = new Button(Text = "Delete", Top = 100, Left = 180, Width = 140, BackColor = Color.Salmon, Font = new Font("Segoe UI", float32 10, FontStyle.Bold))

    let lblSearch = new Label(Text = "Search:", Top = 160, Left = 20, Width = 80, Font = new Font("Segoe UI", float32 10, FontStyle.Bold), ForeColor = Color.White)
    let searchInput = new TextBox(Width = 200, Top = 160, Left = 120, Font = new Font("Segoe UI", float32 10))

    let searchButton = new Button(Text = "Search", Top = 160, Left = 340, Width = 120, BackColor = Color.SkyBlue, Font = new Font("Segoe UI", float32 10, FontStyle.Bold))

    let resultBox = new TextBox(Width = 440, Height = 200, Top = 220, Left = 20, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", float32 10), BackColor = Color.WhiteSmoke)

    let saveButton = new Button(Text = "Save to File", Top = 440, Left = 20, Width = 440, BackColor = Color.Gold, Font = new Font("Segoe UI", float32 10, FontStyle.Bold))

    do
        // form properties to gui
        this.Text <- "Digital Dictionary"
        this.Width <- 500
        this.Height <- 530
        this.BackColor <- Color.FromArgb(50, 50, 70)  // dark background
        this.StartPosition <- FormStartPosition.CenterScreen

        // Add gui elements
        this.Controls.AddRange([|
            lblWord; wordInput; lblDefinition; definitionInput; addButton; deleteButton
            lblSearch; searchInput; searchButton
            resultBox; saveButton
        |])

        // load dictionary on startup
        this.Load.Add(fun _ -> this.loadDictionary())
        this.FormClosing.Add(fun _ -> this.saveToFile())

        // button events
        addButton.Click.Add(fun _ -> this.addOrUpdateWord())
        deleteButton.Click.Add(fun _ -> this.deleteWord())
        searchButton.Click.Add(fun _ -> this.searchWord())
        saveButton.Click.Add(fun _ -> this.saveToFile())

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

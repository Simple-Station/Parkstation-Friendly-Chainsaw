# Guide to Codebase Change Markers

Whenever you change a file not owned by this codebase you need to clarify which portions have been changed.
If the markers described below don't have a description for the file type you're working with, don't add a marker yet and ask a reviewer if you need one and how it should be done.

The following is the annotation formatting per language:


### C#

Single line changes:

```csharp
<Rest of the unchanged file above...>
[Your code here. This can be entirely original or modified from the original file] // Parkstation-[TitleOfFeature] // Optional clarification of why this change was done
<Rest of the unchanged file below...>
```

Multiple line changes:

```csharp
<Rest of the unchanged file above...>
// Parkstation-[TitleOfFeature]-Start // Optional clarification of why this change was done
[Your code here. This can be entirely original or modified from the original file]
// Parkstation-[TitleOfFeature]-End
<Rest of the unchanged file below...>
```


### XAML

Any amount of lines changed:

```xaml
<Rest of the unchanged file above...>
<!-- Parkstation-[TitleOfFeature]-Start -- Optional clarification of why this change was done -->
[Your code here. This can be entirely original, or modified from the original file]
<!-- Parkstation-[TitleOfFeature]-End -->
<Rest of the unchanged file below...>
```



### YML

Single line changes:

```yml
<Rest of the unchanged file above...>
[Your code here. This can be entirely original, or modified from the original file] # Parkstation-[TitleOfFeature] # Optional clarification of why this change was done
<Rest of the unchanged file below...>
```

Multiple line changes:

```yml
<Rest of the unchanged file above...>
# Parkstation-[TitleOfFeature]-Start # Optional clarification of why this change was done
[Your code here. This can be entirely original, or modified from the original file]
# Parkstation-[TitleOfFeature]-End
<Rest of the unchanged file below...>
```


### Never Needed

- MD
- TXT
- XML
- JSON
- RGA (Robust Generic Attribution, specifies asset licenses)
- PNG (Licensed differently)
- OGG (Licensed differently)

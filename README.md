# StreamingUrlFetcherRestServer

## Description

On call this C# web api interacts with a cli-tool called AnimDL to get video sources, processes the output and then sends it to the requester.


## Structure/Dependencies [mandatory]

1. [RestVideoStream](https://github.com/liebki/RestVideoStream) - The swift app which uses the REST API to get data to display
2. [justfoolingaround/AnimDL](https://github.com/justfoolingaround/animdl) - The project allows to get direct links to stream video source from, 


## Paths

### IP:PORT/api/Command/Test (GET)
- **Description:** Returns "zweihundert" to indicate availability.
- **Input:** None
- **Output:** string

### IP:PORT/api/Command/GetStreamIndexes (GET)
- **Description:** Returns a list of available series and their indexes related to the provided "seriesName". This endpoint should be called first to obtain the appropriate index before using GetStreamEpisodes.
- **Input:** seriesName (string)
- **Output:** string

### IP:PORT/api/Command/GetStreamEpisodes (GET)
- **Description:** Returns video source links for a specific series and index.
- **Input:** seriesName (string), streamIndex (integer)
- **Output:** string


## Security
- There are no things implemented to secure the API, everybody could call it and use your bandwith or put you in danger, so if you use it watch the connections, implement security related changes or wait for me to implement something (like a key etc.).
- Right now the api runs with http not https so use it on your own risk.

## Installation/Usage
- Get the code, build and run it but also adjust the path for this API in the swift app's code


## Disclaimer

This project may utilize components allowing access to copyrighted materials (AnimDL). We do not endorse or condone any illegal activities, this project only uses them for testing purposes. Users are responsible for compliance with applicable laws. We disclaim liability for misuse of the project. Please respect copyright laws and the copyright of creators/authors!


## License

This addon is licensed under the [MIT License](https://choosealicense.com/licenses/mit/).


## To-Do
- Add more sources for video material, this counts for the API and the swift app

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Main : MonoBehaviour
{
    public static int[] content = { -1, 3, 2, -1, 1, 1, -1 }; //this given value is a placeholder
    public static int startIndex = 1; //this given value is a placeholder
    public static string algorithmName = "sum"; //this given value is a placeholder
    public static string[] algorithm;

    public string preAlgName = "placeHolder";
    public bool preAlgNameSet;
    public string preStatus = "placeHolder";
    public bool preStatusSet;

    public int currentIndex; //content
    public int currentLine; //algorithm
    public string currentStatus;
    public bool statusUpdated;
    public bool needAlgorithmUpdate;

    public bool isStopped;
    public bool executeOnce;
    public bool executeContinuously;
    public bool executeFast;
    public bool executeVeryFast;
    public bool errorOnVeryFast;
    
    /// <summary>
    /// preset
    /// </summary>
    public float continuousExecutionTime;
    public float continuousExecutionTimer;

    public GameObject gridPrefab;
    /// <summary>
    /// add "{returnIndex},{returnAlgorithmName}"
    /// </summary>
    public List<string> returnAlg = new();

    public bool openPersistentDataPath;
    public string appPersDataPath;

    public static bool logExecution = false;

    public Button leftButton, rightButton;
    private Dictionary<string, string[]> _loadedAlgorithms;
    private void LoadAlgorithm()
    {
        if (!preAlgNameSet)
        {
            preAlgNameSet = true;
            preAlgName = algorithmName;
        }
        //Debug.Log($"Trying to load from loadedAlgorithms: {algorithmName}");
        if (_loadedAlgorithms.TryGetValue(algorithmName, out algorithm))
        {
            //Debug.Log($"Loaded {algorithmName}.");
            currentStatus = ReplaceFirst(algorithm[0], "startAt ", "");
            needAlgorithmUpdate = false;
            return;
        }
        string algorithmFilePath = appPersDataPath + $"/algorithms/{algorithmName}.txt";
        if (!File.Exists(algorithmFilePath))
        {
            string temp = algorithmName;
            algorithmName = preAlgName;
            HandleError($"알고리즘({temp})을 찾을 수 없습니다.",
                "이름을 잘못 입력했거나, 알고리즘을 잘못된 방법으로 생성했습니다. 도움말을 참고하세요.");
            return;
        }
        algorithm = File.ReadAllText(algorithmFilePath).Replace("\r", "").Split("\n");
        if (algorithm[0].StartsWith("startAt ")) { currentStatus = ReplaceFirst(algorithm[0], "startAt ", ""); }
        else
        {
            currentLine = 0;
            currentStatus = "";
            HandleError("startAt 키워드가 존재하지 않습니다.", "startAt을 입력하지 않았거나, 첫 줄에 주석이 있습니다.\n첫 줄에는 항상 startAt 키워드가 있어야 합니다.");
            return;
        }
        _loadedAlgorithms.Add(algorithmName, (string[]) algorithm.Clone());
        needAlgorithmUpdate = false;
    }

    private void Start()
    {
        currentIndex = startIndex;
        currentLine = 0;
        isStopped = false;
        statusUpdated = true;
        needAlgorithmUpdate = true;
        _evfThreadGen = false;
        endVft = false;
        continuousExecutionTimer = continuousExecutionTime;
        appPersDataPath = Application.persistentDataPath;
        _loadedAlgorithms = new();
        _gotoLoop = 0;
        for(int i = 0; i < content.Length; i++)
        {
            GameObject newGrid = Instantiate(gridPrefab);
            newGrid.name = $"grid{i}";
            newGrid.GetComponent<GridManager>().content = content[i];
            newGrid.GetComponent<GridManager>().index = i;
            newGrid.transform.position = new Vector3 (i, 0, 0);
        }
    }

    private bool _evfThreadGen;

    private void LoopExe()
    {
        while (!isStopped)
        {
            try {
                Execute();
            } catch(StackOverflowException) {
                HandleError("반복이 종료되지 않음을 감지했습니다.", "goto 코드가 무한히 실행되고 있거나 연속해서 너무 많이 실행되었습니다. 빠른 실행을 사용하지 않으면 문제가 해결될 수도 있습니다.");
            }
        }
    }

    public static Thread veryFastThread;
    public static bool endVft;

    private void Update()
    {
        if(openPersistentDataPath) { openPersistentDataPath = false; Process.Start(appPersDataPath); }
        if (errorOnVeryFast)
        {
            errorOnVeryFast = false;
            executeVeryFast = false;
            HandleError(_handleErrDesc, _handleErrCause, _handleErrLineNum);
            return;
        }
        if (executeVeryFast && endVft) return;
        if(isStopped) {
            leftButton.interactable = rightButton.interactable = true;
            return;
        }
        if (executeVeryFast)
        {
            if (_evfThreadGen) return;
            _evfThreadGen = true;
            veryFastThread = new Thread(LoopExe);
            veryFastThread.Start();
            return;
        }
        if (!(executeOnce || executeContinuously || executeFast)) return;
        executeOnce = false;
        if(executeContinuously) {
            if(continuousExecutionTimer <= 0.0f)
            {
                continuousExecutionTimer = continuousExecutionTime;
            } else
            {
                continuousExecutionTimer -= Time.deltaTime;
                return;
            }
        }
        
        try {
            Execute();
        } catch(StackOverflowException) {
            HandleError("반복이 종료되지 않음을 감지했습니다.", "goto 코드가 무한히 실행되고 있거나 연속해서 너무 많이 실행되었습니다. 빠른 실행을 사용하지 않으면 문제가 해결될 수도 있습니다.");
        }
    }
    /// <summary>
    /// Get ready for Execute()
    /// </summary>
    /// <returns>If PreExecution was successful (== Execute() is available)</returns>
    private bool PreExecute()
    {
        //변경 시 알고리즘 로드
        if (needAlgorithmUpdate) LoadAlgorithm();

        //실행해야 할 line으로 이동 (상태 변경 시)
        if (statusUpdated)
        {
            if (!preStatusSet)
            {
                preStatusSet = true;
                preStatus = currentStatus;
            }
            int errorDisplayLine = currentLine;
            currentLine = 0;
            try { while (!algorithm[currentLine].Equals($":{currentStatus}")) currentLine++; }
            catch (IndexOutOfRangeException) { 
                string temp = currentStatus;
                currentStatus = preStatus;
                HandleError($"상태({temp})를 찾을 수 없습니다.","없는 상태나 다른 알고리즘의 상태로 이동하려고 시도했습니다.", errorDisplayLine); 
                return false; 
            }
            statusUpdated = false;
        }

        currentLine++;
        return true;
    }

    private int _gotoLoop = 0;

    private void Execute()
    {
        bool preResult = PreExecute();
        if (!preResult) return;
        if (executeVeryFast && endVft) return;
        String line = algorithm[currentLine];

        if (line.Equals("stop"))
        {
            if(logExecution) Debug.Log("stop");
            isStopped = true;
            return;
        }

        if (line.Equals("end"))
        {
            if (logExecution) Debug.Log("end");
            if (returnAlg.Count == 0)
            {
                isStopped = true;
                return;
            }

            needAlgorithmUpdate = true;
            currentLine = int.Parse(returnAlg.ToArray()[returnAlg.Count - 1].Split(",,,")[0]);
            algorithmName = returnAlg.ToArray()[returnAlg.Count - 1].Split(",,,")[1];
            returnAlg.RemoveAt(returnAlg.Count - 1);
            continuousExecutionTimer = 0;
            if (!executeContinuously && !executeFast) executeOnce = true;
            if (executeFast) Update();
            return;
        }
        
        if (line.StartsWith("goto "))
        {
            _gotoLoop++;
            if (_gotoLoop > 2000000)
            {
                HandleError("반복이 종료되지 않음을 감지했습니다.",
                    "goto 코드가 무한히 실행되고 있거나 연속해서 너무 많이 (200만 번 이상) 실행되었습니다. 빠른 실행을 사용하지 않으면 문제가 해결될 수도 있습니다.");
                return;
            }

            if (logExecution) Debug.Log("goto");
            currentStatus = ReplaceFirst(line, "goto ", "");
            statusUpdated = true;
            continuousExecutionTimer = 0;
            if ((!executeContinuously) && (!executeFast)) executeOnce = true;
            return;
        }

        _gotoLoop = 0;

        if(line.StartsWith("alg "))
        {
            if (logExecution) Debug.Log("alg");
            preAlgName = algorithmName;
            returnAlg.Add($"{currentLine},,,{algorithmName}");
            algorithmName = ReplaceFirst(line, "alg ", "");
            needAlgorithmUpdate = true;
            statusUpdated = true;
            continuousExecutionTimer = 0;
            if ((!executeContinuously) && (!executeFast)) executeOnce = true;
            if (executeFast) Update();
            return;
        }

        if (line.StartsWith("//") || line.Equals(""))
        {
            if (logExecution) Debug.Log("comment");
            continuousExecutionTimer = 0;
            if ((!executeContinuously) && (!executeFast)) executeOnce = true;
            if (executeFast) Update();
            return;
        }

        if (logExecution) Debug.Log("일반 명령");
        String[] lineSplitWithArrow = line.Split("->");
        if (lineSplitWithArrow.Length != 2)
        {
            HandleError("잘못된 일반 명령입니다.", "일반 명령에 ->가 없거나, 2개 이상 포함되어 있습니다.", currentLine);
            return;
        }
        string condS = lineSplitWithArrow[0];

        string[] param = lineSplitWithArrow[1].Split(",");
        if (param.Length != 3)
        {
            HandleError("잘못된 일반 명령입니다.", "일반 명령에 변경값, 헤더이동방향, 상태이름 중 하나가 주어지지 않았거나, 그 이외의 값이 주어졌습니다.", currentLine);
            return;
        }
        int condition;
        if (condS.Equals("b")) condition = -1;
        else
        {
            if (!int.TryParse(condS, out condition))
            {
                HandleError("잘못된 일반 명령입니다.", $"주어진 조건 값({condS})이 올바른 값이 아닙니다.", currentLine);
                return;
            }
        }
        int currentContent;
        try
        {
            currentContent = content[currentIndex];
        }
        catch (IndexOutOfRangeException)
        {
            HandleError("헤더의 위치가 가능한 범위를 벗어났습니다.", "헤더의 위치가 0에서 {격자의 수-1} 을 벗어났습니다.\n격자 초기 상태의 시작과 끝에 b를 추가하는 것이 도움될 수 있습니다.");
            return;
        }
        if (currentContent == condition)
        {
            if (param[0].Equals("b"))
            {
                content[currentIndex] = -1;
            }
            else
            {
                try
                {
                    int changeVal = int.Parse(param[0]);
                    if (changeVal is < 0 or > 9) throw new FormatException();
                    content[currentIndex] = changeVal;
                }
                catch (FormatException)
                {
                    HandleError("올바른 입력이 아닙니다!", $"일반 명령의 변경값({param[0]})이 올바르지 않습니다.", currentLine);
                    return;
                }
            }

            if (param[1].ToUpper().Equals("L"))
            {
                currentIndex--;
            } else if (param[1].ToUpper().Equals("R"))
            {
                currentIndex++;
            } else
            {
                HandleError("일반 명령의 헤더이동방향이 잘못되었습니다.", "헤더이동방향 값이 L, R, l, r이 아닙니다.");
                return;
            }

            statusUpdated = true;
            currentStatus = param[2];
        } else
        {
            continuousExecutionTimer = 0;
            if ((!executeContinuously) && (!executeFast)) executeOnce = true;
            // if (executeFast) Update();
        }
    }
    public static string ReplaceFirst(string source, string find, string replace)
    {
        int index = source.IndexOf(find, StringComparison.Ordinal);
        return index < 0 ? source : source[..index] + replace + source[(index + find.Length)..];
    }

    private string _handleErrDesc, _handleErrCause; int _handleErrLineNum;
    /// <summary>
    /// set cause to "internal" to hide needless UI.
    /// </summary>
    private void HandleError(string errorDescription, string cause, int lineNum)
    {
        if (executeVeryFast)
        {
            errorOnVeryFast = true;
            isStopped = true;
            _handleErrDesc = errorDescription;
            _handleErrCause = cause;
            _handleErrLineNum = lineNum;
            return;
        }
        ErrorManager.errorDescription = errorDescription;
        ErrorManager.cause = cause;
        string contentStr = "";
        foreach (int i in content) contentStr += i == -1 ? "b" : $"{i}";
        
        ErrorManager.contentStr = contentStr;
        ErrorManager.algName = algorithmName;
        ErrorManager.algStatus = currentStatus;
        ErrorManager.algLine = (lineNum + 1) + "";
        ErrorManager.algIndex = currentIndex + "";
        SceneManager.LoadScene("ErrorScene");
    }
    /// <summary>
    /// set cause to "internal" to hide needless UI.
    /// </summary>
    private void HandleError(string errorDescription, string cause)
    {
        HandleError(errorDescription, cause, currentLine);
    }
    /// <summary>
    /// set cause to "internal" to hide needless UI.
    /// </summary>
    public static void HandleError_NonAlg(string errorDescription, string cause)
    {
        int[] blank = { };
        HandleError_NonAlg(errorDescription, cause, blank);
    }
    /// <summary>
    /// set cause to "internal" to hide needless UI.
    /// </summary>
    public static void HandleError_NonAlg(string errorDescription, string cause, IEnumerable<int> errContent)
    {
        ErrorManager.errorDescription = errorDescription;
        ErrorManager.cause = cause;
        string contentStr = "";
        foreach(int i in errContent) 
            contentStr += i == -1 ? "b" : $"{i}";
        
        ErrorManager.contentStr = contentStr;
        ErrorManager.algName = "";
        ErrorManager.algStatus = "";
        ErrorManager.algLine = "";
        ErrorManager.algIndex = "";
        SceneManager.LoadScene("ErrorScene");
    }
}

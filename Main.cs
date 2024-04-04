using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Main : MonoBehaviour
{
    public static int[] content = { -1, 3, 2, -1, 1, 1, -1 }; //this given value is a placeholder
    public static int startIndex = 1; //this given value is a placeholder
    public static string algorithmName = "sum"; //this given value is a placeholder
    public static string[] algorithmStringBased;
    public static Algorithm algorithm;

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

    private readonly bool _useStringBasedExecution = false; //very slow
    
    /// <summary>
    /// preset
    /// </summary>
    public float continuousExecutionTime;
    public float continuousExecutionTimer;

    public GameObject gridPrefab;
    /// <summary>
    /// add "{returnIndex},{returnAlgorithmName}"
    /// </summary>
    public List<string> returnAlgStringBased = new();

    public List<KeyValuePair<int, string>> returnAlg;

    public bool openPersistentDataPath;
    public string appPersistentDataPath;

    public static bool logExecution = false;

    public Button leftButton, rightButton;
    private Dictionary<string, string[]> _loadedAlgorithmsStringBased;
    private Dictionary<string, Algorithm> _loadedAlgorithms;
    
    //achievement
    public static int[] answer;
    public static int successCnt;
    public static int requiredSuccess;
    public static bool isAchieveMode, allowAlgCommand, achieveConditionMet;
    
    private bool LoadAlgorithm()
    {
        if(_useStringBasedExecution) { LoadAlgorithm_StringBased(); return true; }
        if (!preAlgNameSet)
        {
            preAlgNameSet = true;
            preAlgName = algorithmName;
        }

        if (_loadedAlgorithms.TryGetValue(algorithmName, out algorithm))
        {
            currentStatus = algorithm.startStatus;
            needAlgorithmUpdate = false;
            return true;
        }
        
        string algorithmFilePath = appPersistentDataPath + $"/algorithms/{algorithmName}.txt";
        if (!File.Exists(algorithmFilePath))
        {
            string temp = algorithmName;
            algorithmName = preAlgName;
            HandleError($"알고리즘({temp})을 찾을 수 없습니다.",
                "이름을 잘못 입력했거나, 알고리즘을 잘못된 방법으로 생성했습니다. 도움말을 참고하세요.");
            return false;
        }
        string[] algLines = File.ReadAllText(algorithmFilePath).Replace("\r", "").Split("\n");
        algorithm = new Algorithm(algLines, algorithmName, out bool algGenSuccess);
        if (!algGenSuccess) return false;
        currentStatus = algorithm.startStatus;
        _loadedAlgorithms.Add(algorithmName, algorithm);
        needAlgorithmUpdate = false;
        return true;
    }

    private void LoadAlgorithm_StringBased()
    {
        if (!preAlgNameSet)
        {
            preAlgNameSet = true;
            preAlgName = algorithmName;
        }
        //Debug.Log($"Trying to load from loadedAlgorithms: {algorithmName}");
        if (_loadedAlgorithmsStringBased.TryGetValue(algorithmName, out algorithmStringBased))
        {
            //Debug.Log($"Loaded {algorithmName}.");
            currentStatus = ReplaceFirst(algorithmStringBased[0], "startAt ", "");
            needAlgorithmUpdate = false;
            return;
        }
        string algorithmFilePath = appPersistentDataPath + $"/algorithms/{algorithmName}.txt";
        if (!File.Exists(algorithmFilePath))
        {
            string temp = algorithmName;
            algorithmName = preAlgName;
            HandleError($"알고리즘({temp})을 찾을 수 없습니다.",
                "이름을 잘못 입력했거나, 알고리즘을 잘못된 방법으로 생성했습니다. 도움말을 참고하세요.");
            return;
        }
        algorithmStringBased = File.ReadAllText(algorithmFilePath).Replace("\r", "").Split("\n");
        if (algorithmStringBased[0].StartsWith("startAt ")) { currentStatus = ReplaceFirst(algorithmStringBased[0], "startAt ", ""); }
        else
        {
            currentLine = 0;
            currentStatus = "";
            HandleError("startAt 키워드가 존재하지 않습니다.", "startAt을 입력하지 않았거나, 첫 줄에 주석이 있습니다.\n첫 줄에는 항상 startAt 키워드가 있어야 합니다.");
            return;
        }
        _loadedAlgorithmsStringBased.Add(algorithmName, (string[]) algorithmStringBased.Clone());
        needAlgorithmUpdate = false;
    }

    private void Start()
    {
        achieveConditionMet = true;
        currentIndex = startIndex;
        currentLine = 0;
        isStopped = false;
        statusUpdated = true;
        needAlgorithmUpdate = true;
        _evfThreadGen = false;
        endVft = false;
        continuousExecutionTimer = continuousExecutionTime;
        appPersistentDataPath = Application.persistentDataPath;
        if(_useStringBasedExecution) _loadedAlgorithmsStringBased = new Dictionary<string, string[]>();
        _loadedAlgorithms = new Dictionary<string, Algorithm>();
        gotoLoop = 0;
        TextChange.algPath = algorithmName;
        returnAlg = new List<KeyValuePair<int, string>>();
        if (isAchieveMode)
        {
            executeVeryFast = true;
            TextChange.achieveTestResult = $"채점 중: {Math.Round(successCnt/(double)requiredSuccess*100.0)}%";
        }
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
        if(openPersistentDataPath) { openPersistentDataPath = false; Process.Start(appPersistentDataPath); }
        if (errorOnVeryFast)
        {
            errorOnVeryFast = false;
            executeVeryFast = false;
            HandleError(_handleErrDesc, _handleErrCause, _handleErrLineNum);
            return;
        }
        if (executeVeryFast && endVft) return;
        if(isStopped) {
            if (isAchieveMode)
            {
                if (!achieveConditionMet)
                {
                    TextChange.achieveTestResult = "조건을 만족하지 않았습니다";
                    SceneManager.LoadScene("AchieveScene");
                    return;
                }
                List<long> output = new();
                foreach (int i in content)
                {
                    if (i == -1)
                    {
                        output.Add(0);
                        continue;
                    }
                    output[^1] *= 10;
                    output[^1] += i;
                }

                for (int i = 0; i < answer.Length; i++)
                {
                    if (answer[i] == -1) continue;
                    if (answer[i] != output[i])
                    {
                        TextChange.achieveTestResult = "틀렸습니다";
                        SceneManager.LoadScene("AchieveScene");
                        return;
                    }
                }
                
                successCnt++;

                if (successCnt >= requiredSuccess)
                {
                    TextChange.achieveTestResult = "맞았습니다!!";
                    SceneManager.LoadScene("AchieveScene");
                    return;
                }
                ButtonScript.needAchieveTest = true;
                SceneManager.LoadScene("AchieveScene");
            }
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
        if (_useStringBasedExecution) { return PreExecuteStringBased(); }

        bool algorithmLoadSuccess = true;
        if (needAlgorithmUpdate) algorithmLoadSuccess = LoadAlgorithm();
        if (!algorithmLoadSuccess) return false;
        if (statusUpdated)
        {
            if (!preStatusSet)
            {
                preStatusSet = true;
                preStatus = currentStatus;
            }
            
            int errorDisplayLine = currentLine;
            string errorDisplayStatus = currentStatus;
            if (!algorithm.statusIndexes.TryGetValue(currentStatus, out currentLine))
            {
                currentStatus = preStatus;
                HandleError($"상태({errorDisplayStatus})를 찾을 수 없습니다.","없는 상태나 다른 알고리즘의 상태로 이동하려고 시도했습니다.", errorDisplayLine); 
                return false; 
            }
            statusUpdated = false;
        }

        currentLine++;
        return true;
    }
    
    public int gotoLoop;
    
    public void Execute()
    {
        if(_useStringBasedExecution) { ExecuteStringBased(); return; }

        bool preResult = PreExecute();
        if (!preResult) return;
        if (executeVeryFast && endVft) return;
        
        algorithm.GetCommand(currentLine).Execute(this);
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
    public void HandleError(string errorDescription, string cause, int lineNum)
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
    public void HandleError(string errorDescription, string cause)
    {
        HandleError(errorDescription, cause, currentLine);
    }
    private static int[] _blank = { };
    
    /// <summary>
    /// set cause to "internal" to hide needless UI.
    /// </summary>
    public static void HandleError_NonAlg(string errorDescription, string cause, IEnumerable<int> errContent = null, string algName = "",
        string algStatus = "", string algLine = "", string algIndex = "")
    {
        if (errContent == null) errContent = _blank;
        ErrorManager.errorDescription = errorDescription;
        ErrorManager.cause = cause;
        string contentStr = "";
        foreach(int i in errContent) 
            contentStr += i == -1 ? "b" : $"{i}";
        
        ErrorManager.contentStr = contentStr;
        ErrorManager.algName = algName;
        ErrorManager.algStatus = algStatus;
        ErrorManager.algLine = algLine;
        ErrorManager.algIndex = algIndex;
        SceneManager.LoadScene("ErrorScene");
    }  
    private bool PreExecuteStringBased()
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
            try { while (!algorithmStringBased[currentLine].Equals($":{currentStatus}")) currentLine++; }
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
    private void ExecuteStringBased()
    {
        bool preResult = PreExecute();
        if (!preResult) return;
        if (executeVeryFast && endVft) return;
        String line = algorithmStringBased[currentLine];

        if (line.Equals("stop"))
        {
            if(logExecution) Debug.Log("stop");
            isStopped = true;
            return;
        }

        if (line.Equals("end"))
        {
            if (logExecution) Debug.Log("end");
            if (returnAlgStringBased.Count == 0)
            {
                isStopped = true;
                return;
            }

            needAlgorithmUpdate = true;
            currentLine = int.Parse(returnAlgStringBased.ToArray()[returnAlgStringBased.Count - 1].Split(",,,")[0]);
            algorithmName = returnAlgStringBased.ToArray()[returnAlgStringBased.Count - 1].Split(",,,")[1];
            returnAlgStringBased.RemoveAt(returnAlgStringBased.Count - 1);
            continuousExecutionTimer = 0;
            if (!executeContinuously && !executeFast) executeOnce = true;
            if (executeFast) Update();
            return;
        }
        
        if (line.StartsWith("goto "))
        {
            gotoLoop++;
            if (gotoLoop > 2000000)
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

        gotoLoop = 0;

        if(line.StartsWith("alg "))
        {
            if (logExecution) Debug.Log("alg");
            preAlgName = algorithmName;
            returnAlgStringBased.Add($"{currentLine},,,{algorithmName}");
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
        string[] lineSplitWithArrow = line.Split("->");
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
            if (executeFast) Update();
        }
    }
    
}

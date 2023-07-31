using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public static int[] content = { -1, 3, 2, -1, 1, 1, -1 }; //this given value is a placeholder
    public static GameObject[] grids;
    public static int startIndex = 1; //this given value is a placeholder
    public static string algorithmName = "sum"; //this given value is a placeholder
    public static string[] algorithm;

    public string preAlgName = "placeHolder";
    public bool preAlgNameSet = false;
    public string preStatus = "placeHolder";
    public bool preStatusSet = false;

    public int currentIndex; //content
    public int currentLine; //algorithm
    public string currentStatus;
    public bool statusUpdated;
    public bool algorithmUpdated;

    public bool isStopped;
    public bool executeOnce;
    public bool executeContinuously;
    public bool executeFast;
    public bool showResult;

    /// <summary>
    /// preset
    /// </summary>
    public float continuousExecutionTime;
    public float continuousExecutionTimer;

    public GameObject gridPrefab;
    /// <summary>
    /// add "{returnIndex},{returnAlgorithmName}"
    /// </summary>
    public ArrayList returnAlg = new();

    public bool openPersistentDataPath = false;

    public static bool logExecution = false;

    void Start()
    {
        currentIndex = startIndex;
        currentLine = 0;
        isStopped = false;
        statusUpdated = true;
        algorithmUpdated = true;
        grids = new GameObject[content.Length];
        continuousExecutionTimer = continuousExecutionTime;
        for(int i = 0; i < content.Length; i++)
        {
            GameObject newGrid = Instantiate(gridPrefab);
            newGrid.name = $"grid{i}";
            newGrid.GetComponent<GridManager>().content = content[i];
            newGrid.GetComponent<GridManager>().index = i;
            newGrid.transform.position = new Vector3 (i, 0, 0);
        }
    }

    private void Update()
    {
        if(openPersistentDataPath) { openPersistentDataPath = false; Process.Start(Application.persistentDataPath); }
        if(isStopped) return;
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

        //�˰��� �ε�(���� ��)
        if (algorithmUpdated)
        {
            if (!preAlgNameSet)
            {
                preAlgNameSet = true;
                preAlgName = algorithmName;
            }
            string algorithmFilePath = Application.persistentDataPath + $"/algorithms/{algorithmName}.txt";
            if (!File.Exists(algorithmFilePath))
            {
                string temp = algorithmName;
                algorithmName = preAlgName;
                HandleError($"�˰���({temp})�� ã�� �� �����ϴ�.",
                    "�̸��� �߸� �Է��߰ų�, �˰����� �߸��� ������� �����߽��ϴ�. ������ �����ϼ���.");
                return;
            }
            algorithm = File.ReadAllText(algorithmFilePath).Replace("\r", "").Split("\n");
            if (algorithm[0].StartsWith("startAt ")) { currentStatus = ReplaceFirst(algorithm[0], "startAt ", ""); }
            else
            {
                currentLine = 0;
                currentStatus = "";
                HandleError("startAt Ű���尡 �������� �ʽ��ϴ�.", "startAt�� �Է����� �ʾҰų�, ù �ٿ� �ּ��� �ֽ��ϴ�.\nù �ٿ��� �׻� startAt Ű���尡 �־�� �մϴ�.");
                return;
            }
            algorithmUpdated = false;
        }

        //�����ؾ� �� line���� �̵� (���� ���� ��)
        if (statusUpdated)
        {
            if (!preStatusSet)
            {
                preStatusSet = true;
                preStatus = currentStatus;
            }
            currentLine = 0;
            try { while (!algorithm[currentLine].Equals($":{currentStatus}")) currentLine++; }
            catch (IndexOutOfRangeException) { 
                string temp = currentStatus;
                currentStatus = preStatus;
                HandleError($"����({temp})�� ã�� �� �����ϴ�.","���� ���³� �ٸ� �˰����� ���·� �̵��Ϸ��� �õ��߽��ϴ�."); 
                return; }
            statusUpdated = false;
        }

        currentLine++;
        Execute(algorithm[currentLine]);

        if (showResult && (!isStopped)) Update();
    }

    public void Execute(string line)
    {
        if (line.Equals("stop"))
        {
            if(logExecution) UnityEngine.Debug.Log("stop");
            isStopped = true;
            return;
        }
        else if (line.Equals("end"))
        {
            if (logExecution) UnityEngine.Debug.Log("end");
            if (returnAlg.Count == 0)
            {
                isStopped = true;
                return;
            } else
            {
                algorithmUpdated = true;
                currentLine = int.Parse(((string)returnAlg.ToArray()[returnAlg.Count - 1]).Split(",,,")[0]);
                algorithmName = ((string)returnAlg.ToArray()[returnAlg.Count - 1]).Split(",,,")[1];
                returnAlg.RemoveAt(returnAlg.Count - 1);
                continuousExecutionTimer = 0;
                if ((!executeContinuously) && (!executeFast)) executeOnce = true;
                if (executeFast) Update();
                return;
            }
        }
        else if (line.StartsWith("goto "))
        {
            if (logExecution) UnityEngine.Debug.Log("goto");
            currentStatus = ReplaceFirst(line, "goto ", "");
            statusUpdated = true;
            continuousExecutionTimer = 0;
            if ((!executeContinuously) && (!executeFast)) executeOnce = true;
            if (executeFast) Update();
            return;
        }
        else if(line.StartsWith("alg "))
        {
            if (logExecution) UnityEngine.Debug.Log("alg");
            preAlgName = algorithmName;
            returnAlg.Add($"{currentLine},,,{algorithmName}");
            algorithmName = ReplaceFirst(line, "alg ", "");
            algorithmUpdated = true;
            statusUpdated = true;
            continuousExecutionTimer = 0;
            if ((!executeContinuously) && (!executeFast)) executeOnce = true;
            if (executeFast) Update();
        }
        else if (line.StartsWith("//") || line.Equals(""))
        {
            if (logExecution) UnityEngine.Debug.Log("comment");
            continuousExecutionTimer = 0;
            if ((!executeContinuously) && (!executeFast)) executeOnce = true;
            if (executeFast) Update();
            return;
        }
        else
        {
            if (logExecution) UnityEngine.Debug.Log("�Ϲ� ���");
            string condS = line.Split("->")[0];
            string[] param = line.Split("->")[1].Split(",");
            int condition;
            if (condS.Equals("b")) condition = -1;
            else { condition = int.Parse(condS); }
            int currentContent;
            try
            {
                currentContent = content[currentIndex];
            }
            catch (IndexOutOfRangeException)
            {
                HandleError("����� ��ġ�� ������ ������ ������ϴ�.", "����� ��ġ�� 0���� {������ ��-1} �� ������ϴ�.\n���� �ʱ� ������ ���۰� ���� b�� �߰��ϴ� ���� ����� �� �ֽ��ϴ�.");
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
                    content[currentIndex] = int.Parse(param[0]);
                }

                if (param[1].ToUpper().Equals("L"))
                {
                    currentIndex--;
                } else if (param[1].ToUpper().Equals("R"))
                {
                    currentIndex++;
                } else
                {
                    HandleError("�Ϲ� ����� ����̵������� �߸��Ǿ����ϴ�.", "����̵����� ���� L, R, l, r�� �ƴմϴ�.");
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
    public static string ReplaceFirst(string source, string find, string replace)
    {
        int index = source.IndexOf(find);
        return index < 0 ? source : source[..index] + replace + source[(index + find.Length)..];
    }
    /// <summary>
    /// set cause to "internal" to hide needless UI.
    /// </summary>
    public void HandleError(string errorDescription, string cause)
    {
        ErrorManager.errorDescription = errorDescription;
        ErrorManager.cause = cause;
        string contentStr = "";
        foreach (int i in content)
        {
            if (i == -1)
            {
                contentStr += "b";
            }
            else
            {
                contentStr += $"{i}";
            }
        }
        ErrorManager.contentStr = contentStr;
        ErrorManager.algName = algorithmName;
        ErrorManager.algStatus = currentStatus;
        ErrorManager.algLine = (currentLine + 1) + "";
        ErrorManager.algIndex = currentIndex + "";
        showResult = false;
        SceneManager.LoadScene("ErrorScene");
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
    public static void HandleError_NonAlg(string errorDescription, string cause, int[] content)
    {
        ErrorManager.errorDescription = errorDescription;
        ErrorManager.cause = cause;
        string contentStr = "";
        foreach(int i in content)
        {
            if(i == -1)
            {
                contentStr += "b";
            } else
            {
                contentStr += $"{i}";
            }
        }
        ErrorManager.contentStr = contentStr;
        ErrorManager.algName = "";
        ErrorManager.algStatus = "";
        ErrorManager.algLine = "";
        ErrorManager.algIndex = "";
        SceneManager.LoadScene("ErrorScene");
    }
}
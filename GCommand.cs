using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Algorithm
{
    public string algName;
    private List<GCommand> _algorithms;
    public Dictionary<string, int> statusIndexes;
    public string startStatus;
    private readonly bool _doLog = false;

    public Algorithm(string[] lines, string algorithmName, out bool success)
    {
        statusIndexes = new Dictionary<string, int>();
        _algorithms = new List<GCommand>();
        algName = algorithmName;
        startStatus = null;
        int linesCount = lines.Length;
        success = true;
        for (int i = 0; i < linesCount; i++)
        {
            if (lines[i].StartsWith("startAt "))
            {
                if(_doLog) Debug.Log("startAt");
                string statusName = Main.ReplaceFirst(lines[i], "startAt ", "");
                if (!IsValidName(statusName))
                {
                    Main.HandleError_NonAlg("잘못된 상태 이름입니다.", "startAt에 잘못된 상태 이름이 주어졌습니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
                if (startStatus != null)
                {
                    Main.HandleError_NonAlg("startAt 명령이 여러 개 있습니다.", "startAt 명령은 하나만 있어야 합니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
                startStatus = statusName;
                _algorithms.Add(new GCommand(GCommand.StartAt));
                continue;
            }
            
            if(lines[i].StartsWith(":"))
            {
                if(_doLog) Debug.Log(":");
                string statusName = Main.ReplaceFirst(lines[i], ":", "");
                if (!IsValidName(statusName))
                {
                    Main.HandleError_NonAlg("잘못된 상태 이름입니다.", "상태 이름에 0-9, a-z, A-Z, _ 이외의 문자가 들어갔습니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
                statusIndexes.Add(statusName, i);
                _algorithms.Add(new GCommand(GCommand.StatusDeclaration));
                continue;
            }

            if (lines[i].StartsWith("end"))
            {
                if(_doLog) Debug.Log("end");
                if (!lines[i].Replace(" ", "").Equals("end"))
                {
                    Main.HandleError_NonAlg("잘못된 end 명령입니다.", "end 명령에 추가적인 인수가 주어졌습니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
                _algorithms.Add(new EndCommand());
                continue;
            }
            
            if (lines[i].StartsWith("goto"))
            {
                if(_doLog) Debug.Log("goto");
                string gotoStatusName = Main.ReplaceFirst(lines[i], "goto ", "");
                if (!IsValidName(gotoStatusName))
                {
                    Main.HandleError_NonAlg("잘못된 상태 이름입니다.", "상태 이름에 0-9, a-z, A-Z, _ 이외의 문자가 들어갔습니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
                _algorithms.Add(new GotoCommand(gotoStatusName));
                continue;
            }
            
            if (lines[i].StartsWith("alg"))
            {
                if(_doLog) Debug.Log("alg");
                string algAlgName = Main.ReplaceFirst(lines[i], "alg ", "");
                if (!IsValidName(algAlgName))
                {
                    Main.HandleError_NonAlg("잘못된 알고리즘 이름입니다.", "알고리즘 이름에 0-9, a-z, A-Z, _ 이외의 문자가 들어갔습니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
                _algorithms.Add(new AlgCommand(algAlgName));
                continue;
            }

            if (lines[i].StartsWith("stop"))
            {
                if(_doLog) Debug.Log("stop");
                if (!lines[i].Replace(" ", "").Equals("stop"))
                {
                    Main.HandleError_NonAlg("잘못된 stop 명령입니다.", "stop 명령에 추가적인 인수가 주어졌습니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
                _algorithms.Add(new StopCommand());
                continue;
            }

            if (lines[i].Trim().Equals("") || lines[i].StartsWith("//"))
            {
                if(_doLog) Debug.Log("comment");
                _algorithms.Add(new GCommand(GCommand.Comment));
                continue;
            }
            
            //일반 명령
            string[] lineSplitWithArrow = lines[i].Split("->");
            if (lineSplitWithArrow.Length != 2)
            {
                Main.HandleError_NonAlg("잘못된 일반 명령입니다.", "일반 명령에 ->가 없거나, 2개 이상 포함되어 있습니다.", null, algName, "", $"{i}");
                success = false;
                return;
            }
            string condS = lineSplitWithArrow[0];

            string[] param = lineSplitWithArrow[1].Split(",");
            if (param.Length != 3)
            {
                Main.HandleError_NonAlg("잘못된 일반 명령입니다.", "일반 명령에 변경값, 헤더이동방향, 상태이름 중 하나가 주어지지 않았거나, 그 이외의 값이 주어졌습니다.", null, algName, "", $"{i}");
                success = false;
                return;
            }
            int condition;
            if (condS.Equals("b")) condition = -1;
            else
            {
                if (!int.TryParse(condS, out condition))
                {
                    Main.HandleError_NonAlg("잘못된 일반 명령입니다.", $"주어진 조건 값({condS})이 올바른 값이 아닙니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
            }

            int edit;
            if (param[0].Equals("b"))
            {
                edit = -1;
            }
            else
            {
                try
                {
                    int changeVal = int.Parse(param[0]);
                    if (changeVal is < 0 or > 9) throw new FormatException();
                    edit = changeVal;
                }
                catch (FormatException)
                {
                    Main.HandleError_NonAlg("잘못된 일반 명령입니다.", $"일반 명령의 변경값({param[0]})이 올바르지 않습니다.", null, algName, "", $"{i}");
                    success = false;
                    return;
                }
            }

            bool dir;
            if (param[1].ToUpper().Equals("L"))
            {
                dir = NormalCommand.Left;
            } else if (param[1].ToUpper().Equals("R"))
            {
                dir = NormalCommand.Right;
            } else
            {
                Main.HandleError_NonAlg("일반 명령의 헤더이동방향이 잘못되었습니다.", "헤더이동방향 값이 L, R, l, r이 아닙니다.", null, algName, "", $"{i}");
                success = false;
                return;
            }
            string normStatusName = param[2];
            if (!IsValidName(normStatusName))
            {
                Main.HandleError_NonAlg("일반 명령의 상태 이름이 잘못되었습니다.", "상태 이름에 이름에 0-9, a-z, A-Z, _ 이외의 문자가 포함되어 있습니다.", null, algName, "", $"{i}");
                success = false;
                return;
            }
            
            _algorithms.Add(new NormalCommand(condition, edit, dir, normStatusName));
        }

        if (startStatus != null) return;
        
        Main.HandleError_NonAlg("startAt 키워드가 존재하지 않습니다.", "startAt을 입력하지 않았습니다.", null, algName);
        success = false;
    }

    public static bool IsValidName(string s)
    {
        foreach (char c in s)
        {
            switch (c)
            {
                case >= '0' and <= '9':
                case >= 'a' and <= 'z':
                case >= 'A' and <= 'Z':
                case '_':
                    continue;
                default:
                    return false;
            }
        }
        return true;
    }

    public GCommand GetCommand(int index)
    {
        if (index < 0 || index >= _algorithms.Count) return null;
        return _algorithms[index];
    }
}
public class GCommand
{
    public const int StartAt = 0,
        Normal = 1,
        End = 2,
        Goto = 3,
        Alg = 4,
        Stop = 5,
        StatusDeclaration = 6,
        Comment = -1;

    public int type;

    public virtual void Execute(Main main)
    {
        main.continuousExecutionTimer = 0;
        if (!main.executeContinuously && !main.executeFast) main.executeOnce = true;
        main.gotoLoop = 0;
    }

    protected GCommand() { }

    public GCommand(int type)
    {
        this.type = type;
    }
}

public class NormalCommand : GCommand
{
    public int condition, edit;
    public bool direction;
    public static readonly bool Left = true, Right = false;
    public string status;

    public NormalCommand(int condition, int edit, bool direction, string status)
    {
        this.condition = condition;
        this.edit = edit;
        this.direction = direction;
        this.status = status;
        type = Normal;
    }

    public override void Execute(Main main)
    {
        if (main.currentIndex < 0 || main.currentIndex >= Main.content.Length)
        {
            main.HandleError("헤더의 위치가 가능한 범위를 벗어났습니다.", "헤더의 위치가 0에서 {격자의 수-1} 을 벗어났습니다.\n격자 초기 상태의 시작과 끝에 b를 추가하는 것이 도움될 수 있습니다.");
            return;
        }
        main.gotoLoop = 0;
        if (Main.content[main.currentIndex] != condition)
        {
            main.continuousExecutionTimer = 0;
            if (!main.executeContinuously && !main.executeFast) main.executeOnce = true;
            return;
        }
        Main.content[main.currentIndex] = edit;
        main.currentIndex += direction == Left ? -1 : +1;
        main.currentStatus = status;
        main.statusUpdated = true;
    }
}

public class EndCommand : GCommand
{
    public EndCommand()
    {
        type = End;
    }

    public override void Execute(Main main)
    {
        main.gotoLoop = 0;
        if (main.returnAlg.Count == 0)
        {
            main.isStopped = true;
            return;
        }

        main.needAlgorithmUpdate = true;
        main.currentLine = main.returnAlg[^1].Key; //Get Last Element
        Main.algorithmName = main.returnAlg[^1].Value;
        main.returnAlg.RemoveAt(main.returnAlg.Count-1);
        main.continuousExecutionTimer = 0;
        if (!main.executeContinuously && !main.executeFast) main.executeOnce = true;
    }
}

public class GotoCommand : GCommand
{
    private readonly string _status;
    public GotoCommand(string status)
    {
        _status = status;
        type = Goto;
    }

    public override void Execute(Main main)
    {
        main.gotoLoop++;
        if (main.gotoLoop > 10000000)
        {
            main.HandleError("반복이 종료되지 않음을 감지했습니다.",
                "goto 코드가 무한히 실행되고 있거나 연속해서 너무 많이 (천만 번 이상) 실행되었습니다.");
            return;
        }

        main.currentStatus = _status;
        main.statusUpdated = true;
        main.continuousExecutionTimer = 0;
        if (!main.executeContinuously && !main.executeFast) main.executeOnce = true;
    }
}

public class AlgCommand : GCommand
{
    public readonly string algName;
    public AlgCommand(string algName)
    {
        type = Alg;
        this.algName = algName;
    }
    
    public override void Execute(Main main)
    {
        main.gotoLoop = 0;
        main.preAlgName = Main.algorithmName;
        main.returnAlg.Add(new KeyValuePair<int, string>(main.currentLine, Main.algorithmName));
        Main.algorithmName = algName;
        main.needAlgorithmUpdate = true;
        main.statusUpdated = true;
        main.continuousExecutionTimer = 0;
        if (!main.executeContinuously && !main.executeFast) main.executeOnce = true;
    }
}

public class StopCommand : GCommand
{
    public StopCommand()
    {
        type = Stop;
    }
    
    public override void Execute(Main main)
    {
        main.gotoLoop = 0;
        main.isStopped = true;
    }
}


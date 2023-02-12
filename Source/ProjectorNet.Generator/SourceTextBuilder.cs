using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ProjectorNet.Generator.Model;

namespace ProjectorNet.Generator;

public class SourceTextBuilder
{
    private static readonly string[] LineSeparators = { "\r\n", "\n" };

    private List<string> _indents = new ()
    {
        "",
        "    ",
        "        ",
        "            ",
        "                ",
        "                    "
    };

    private readonly StringBuilder _builder;

    private int _indentation;

    private bool _mustIndent;

    public SourceTextBuilder(int initialCapacity = 20000)
    {
        _builder = new StringBuilder(initialCapacity);
    }

    public int Indentation
    {
        get => _indentation;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _indentation = value;
            if (_indents.Count <= _indentation)
            {
                for (int i = _indents.Count; i <= _indentation; i++)
                    _indents.Add(new string(' ', i * 4));
            }
        }
    }

    public IDisposable Indent()
    {
        Indentation++;
        return new Disposable(() => Indentation--);
    }

    public void Append(string str)
    {
        var lines = str.Split(LineSeparators, StringSplitOptions.None);
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            AppendIndentation();
            _builder.Append(line);
            if (index != lines.Length - 1)
            {
                AppendLine();
            }
        }
    }

    public void Append(object value)
    {
        AppendIndentation();
        _builder.Append(value);
    }

    public void Append([InterpolatedStringHandlerArgument("")]  ref SourceTextInterpolatedStringHandler handler)
    {
    }

    public void AppendLine()
    {
        _builder.AppendLine();
        _mustIndent = true;
    }

    public void AppendLine(string str)
    {
        Append(str);
        AppendLine();
    }

    public void AppendLine([InterpolatedStringHandlerArgument("")] ref SourceTextInterpolatedStringHandler handler)
    {
        AppendLine();
    }

    public IDisposable BeginBlock(string terminator = "", bool newLine = true)
    {
        AppendLine("{");
        Indentation++;
        return new Disposable(() =>
        {
            Indentation--;
            Append($"}}{terminator}");
            if (newLine)
                AppendLine();
        });
    }

    public IDisposable BeginNamespace(string ns)
    {
        AppendLine($"namespace {ns}");
        return BeginBlock();
    }

    public IDisposable BeginTypeDeclaration(TypeInfo typeInfo, params string[] inheritanceList) =>
        BeginTypeDeclaration(null, typeInfo, inheritanceList);

    public IDisposable BeginTypeDeclaration(string modifiers, TypeInfo typeInfo, params string[] inheritanceList)
    {
        if (!string.IsNullOrWhiteSpace(modifiers))
            Append($"{modifiers} ");

        Append($"partial {TypeKindToString(typeInfo.Kind)} {typeInfo.Name.Name}");
        if (inheritanceList.Length > 0)
        {
            AppendLine(" :");
            using var indent = Indent();
            for (int i = 0; i < inheritanceList.Length; i++)
            {
                AppendLine($"{inheritanceList[i]}{(i == inheritanceList.Length - 1 ? "" : ",")}");
            }
        }
        else
        {
            AppendLine();
        }

        return BeginBlock();
    }

    private void AppendIndentation()
    {
        if (_mustIndent)
        {
            _builder.Append(_indents[Indentation]);
            _mustIndent = false;
        }
    }

    private string TypeKindToString(TypeKind typeKind) =>
        typeKind switch
        {
            TypeKind.Class => "class",
            TypeKind.Struct => "struct",
            TypeKind.RecordClass => "record class",
            TypeKind.RecordStruct => "record struct",
            TypeKind.ReadonlyStruct => "readonly struct",
            TypeKind.ReadonlyRecordStruct => "readonly record struct",
            _ => throw new ArgumentOutOfRangeException(nameof(typeKind), typeKind, null)
        };
    

    public override string ToString() => _builder.ToString();

    [InterpolatedStringHandler]
    public readonly ref struct SourceTextInterpolatedStringHandler
    {
        private readonly SourceTextBuilder _builder;

        public SourceTextInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            SourceTextBuilder builder)
        {
            _builder = builder;
        }

        public void AppendLiteral(string str) => _builder.Append(str);

        public void AppendFormatted(string str) => _builder.Append(str);

        public void AppendFormatted(object obj) => _builder.Append(obj);
    }

    private class Disposable : IDisposable
    {
        private Action _action;

        public Disposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action?.Invoke();
            _action = null;
        }
    }
}

using System.Collections.Generic;

namespace Logicosk;

public record Alternative(
    string Text,
    float Correctness
);

public record Question(
    List<int> Aspects,
    string Text,
    string Image,
    List<Alternative> Alternatives
);

public record Test(
    string ResourceFolder,
    int MinutesDuration,
    List<Question> Questions
);

public record AutoTest(
    List<object> Inputs,
    object Output,
    bool Hidden
);

public record PraticalTest(
    string Text,
    string Example,
    bool Performance,
    string Language,
    List<AutoTest> Tests
);
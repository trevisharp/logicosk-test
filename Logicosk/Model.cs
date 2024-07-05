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
    List<Question> Questions
);
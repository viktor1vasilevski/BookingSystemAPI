using Main.DTOs;
using Main.Enums;

namespace Main.Responses;

public class SearchRes
{
    public List<Option> Options { get; set; }
    public SearchTypeEnum SearchType { get; set; }
}
namespace BuildingBlocks.Requests;

public abstract record PageParameters(int PageSize = 20, int PageNumber = 1);

namespace Fundo.Applications.Apllication.Dtos;

public record PaginationRequestDto(int PageNumber = 1, int PageSize = 10);
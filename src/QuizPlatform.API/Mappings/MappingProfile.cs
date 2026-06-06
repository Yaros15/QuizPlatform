using AutoMapper;
using QuizPlatform.Domain.Entities;
using QuizPlatform.DTO.Requests;
using QuizPlatform.DTO.Responses;

namespace QuizPlatform.API.Mappings
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User ↔ DTO
            CreateMap<User, UserResponse>();
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Пароль хешируем вручную

            // Quiz ↔ DTO
            CreateMap<Quiz, QuizResponse>()
                .ForMember(dest => dest.AuthorUsername, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Username : "Unknown"));

            CreateMap<CreateQuizRequest, Quiz>()
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Puzzle ↔ DTO
            CreateMap<Puzzle, PuzzleResponse>();
            CreateMap<CreatePuzzleRequest, Puzzle>()
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.Quiz, opt => opt.Ignore());

            // AnswerOption ↔ DTO
            CreateMap<AnswerOption, AnswerOptionResponse>();
            CreateMap<CreateAnswerOptionRequest, AnswerOption>()
                .ForMember(dest => dest.PuzzleId, opt => opt.Ignore())
                .ForMember(dest => dest.Puzzle, opt => opt.Ignore());
        }
    }
}

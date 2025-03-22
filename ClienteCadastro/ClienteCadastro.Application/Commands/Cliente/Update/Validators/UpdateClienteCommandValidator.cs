using System;
using FluentValidation;

namespace ClienteCadastro.Application.Commands.Cliente.Update.Validators
{
    public class UpdateClienteCommandValidator : AbstractValidator<UpdateClienteCommand>
    {
        public UpdateClienteCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("O ID do cliente é obrigatório");

            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

            RuleFor(c => c.TipoPessoa)
                .Must(tp => tp == 'F' || tp == 'J')
                .WithMessage("O tipo de pessoa deve ser 'F' para física ou 'J' para jurídica");

            RuleFor(c => c.Documento)
                .NotEmpty().WithMessage("O documento é obrigatório")
                .MaximumLength(18).WithMessage("O documento deve ter no máximo 18 caracteres");

            // Regras específicas para Pessoa Física
            When(c => c.TipoPessoa == 'F', () =>
            {
                RuleFor(c => c.DataNascimento)
                    .Must(dn => dn.AddYears(18) <= DateTime.Now)
                    .WithMessage("O cliente deve ter pelo menos 18 anos");
            });

            // Regras específicas para Pessoa Jurídica
            When(c => c.TipoPessoa == 'J', () =>
            {
                RuleFor(c => c.IsIsentoIE)
                    .Equal(true)
                    .When(c => string.IsNullOrEmpty(c.IE))
                    .WithMessage("É necessário informar a Inscrição Estadual ou marcar como isento");

                RuleFor(c => c.IE)
                    .NotEmpty()
                    .When(c => !c.IsIsentoIE)
                    .WithMessage("É necessário informar a Inscrição Estadual quando não for isento");
            });

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("O email é obrigatório")
                .EmailAddress().WithMessage("Email inválido")
                .MaximumLength(100).WithMessage("O email deve ter no máximo 100 caracteres");

            RuleFor(c => c.Telefone)
                .NotEmpty().WithMessage("O telefone é obrigatório")
                .MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres");

            // Validação de endereços
            RuleFor(c => c.Enderecos)
                .NotEmpty().WithMessage("É necessário informar pelo menos um endereço");

            RuleForEach(c => c.Enderecos).ChildRules(endereco =>
            {
                endereco.RuleFor(e => e.CEP)
                    .NotEmpty().WithMessage("O CEP é obrigatório")
                    .MaximumLength(9).WithMessage("O CEP deve ter no máximo 9 caracteres");

                endereco.RuleFor(e => e.Logradouro)
                    .NotEmpty().WithMessage("O logradouro é obrigatório")
                    .MaximumLength(100).WithMessage("O logradouro deve ter no máximo 100 caracteres");

                endereco.RuleFor(e => e.Numero)
                    .NotEmpty().WithMessage("O número é obrigatório")
                    .MaximumLength(20).WithMessage("O número deve ter no máximo 20 caracteres");

                endereco.RuleFor(e => e.Bairro)
                    .NotEmpty().WithMessage("O bairro é obrigatório")
                    .MaximumLength(50).WithMessage("O bairro deve ter no máximo 50 caracteres");

                endereco.RuleFor(e => e.Cidade)
                    .NotEmpty().WithMessage("A cidade é obrigatória")
                    .MaximumLength(50).WithMessage("A cidade deve ter no máximo 50 caracteres");

                endereco.RuleFor(e => e.Estado)
                    .NotEmpty().WithMessage("O estado é obrigatório")
                    .Length(2).WithMessage("O estado deve ter 2 caracteres");
            });
        }
    }
}

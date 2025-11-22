export const validation = {
  isEmail: (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  },
  isPhone: (phone: string): boolean => {
    // Regex para (XX) 9XXXX-XXXX ou XX9XXXX-XXXX ou 9XXXX-XXXX ou XXXXX-XXXX
    const phoneRegex = /^(\d{2})?\s?9?\s?\d{4}-?\d{4}$/;
    // Remove todos os não-dígitos antes de testar
    return phoneRegex.test(phone.replace(/\D/g, ''));
  },
  isCPF: (cpf: string): boolean => {
    const cleanCPF = cpf.replace(/\D/g, '');

    if (cleanCPF.length !== 11) return false;

    // Elimina CPFs invalidos conhecidos
    if (
      cleanCPF.split('').every((c) => c === cleanCPF[0])
    )
      return false;

    let sum = 0;
    let remainder;

    // 1st digit
    for (let i = 1; i <= 9; i++) {
      sum += parseInt(cleanCPF.substring(i - 1, i)) * (11 - i);
    }
    remainder = (sum * 10) % 11;

    if (remainder === 10 || remainder === 11) remainder = 0;
    if (remainder !== parseInt(cleanCPF.substring(9, 10))) return false;

    sum = 0;
    // 2nd digit
    for (let i = 1; i <= 10; i++) {
      sum += parseInt(cleanCPF.substring(i - 1, i)) * (12 - i);
    }
    remainder = (sum * 10) % 11;

    if (remainder === 10 || remainder === 11) remainder = 0;
    if (remainder !== parseInt(cleanCPF.substring(10, 11))) return false;

    return true;
  },
};
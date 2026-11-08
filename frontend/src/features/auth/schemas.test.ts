import { describe, expect, it } from 'vitest'

import { createUserSchema, resetPasswordSchema } from '@/features/users/schemas'

import { changePasswordSchema, loginSchema, passwordSchema, usernameSchema } from './schemas'

describe('usernameSchema', () => {
  it('accepts letters, digits, dot, dash and underscore', () => {
    expect(usernameSchema.parse(' ana.k_1-x ')).toBe('ana.k_1-x')
  })

  it.each(['ab', 'com espaço', 'nome@x', 'a'.repeat(51)])('rejects %j', (value) => {
    expect(usernameSchema.safeParse(value).success).toBe(false)
  })
})

describe('passwordSchema', () => {
  it('requires 8+ characters with letters and digits', () => {
    expect(passwordSchema.safeParse('abc12345').success).toBe(true)
    expect(passwordSchema.safeParse('abcdefgh').success).toBe(false)
    expect(passwordSchema.safeParse('12345678').success).toBe(false)
    expect(passwordSchema.safeParse('ab1').success).toBe(false)
  })
})

describe('loginSchema', () => {
  it('only requires values (legacy accounts may have short passwords)', () => {
    expect(loginSchema.safeParse({ username: 'admin', password: '123' }).success).toBe(true)
    expect(loginSchema.safeParse({ username: ' ', password: '' }).success).toBe(false)
  })
})

describe('password confirmation', () => {
  it('rejects mismatched confirmation and reuse of the current password', () => {
    const mismatch = changePasswordSchema.safeParse({
      currentPassword: 'old12345',
      newPassword: 'new12345',
      confirmPassword: 'other123',
    })
    expect(mismatch.error?.issues[0]?.path).toEqual(['confirmPassword'])

    const reuse = changePasswordSchema.safeParse({
      currentPassword: 'same1234',
      newPassword: 'same1234',
      confirmPassword: 'same1234',
    })
    expect(reuse.error?.issues[0]?.path).toEqual(['newPassword'])

    expect(
      resetPasswordSchema.safeParse({ newPassword: 'abc12345', confirmPassword: 'abc12345' })
        .success,
    ).toBe(true)
  })
})

describe('createUserSchema', () => {
  it('requires a role', () => {
    const result = createUserSchema.safeParse({
      username: 'operador1',
      fullName: 'Operador Um',
      role: 'Root',
      password: 'abc12345',
    })
    expect(result.error?.issues[0]?.message).toBe('Escolha o perfil.')
  })
})

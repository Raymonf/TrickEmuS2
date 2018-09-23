# TrickEmuS2

## Instructions
TODO. tl;dr check out GameServer\Data.cs.

## Notes
The code is awful.

## Client
Use the latest Korean client. Some packets aren't complete, so you need to mess with the exceptions a little.

Patch the jump instruction (jne/je) to jmp at `Trickster.0+0x74497` assuming you are using the latest Korean client. Use `dummy` as your user password if you don't know how to patch that.

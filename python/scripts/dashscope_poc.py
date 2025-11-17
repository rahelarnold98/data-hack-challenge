# example usage:
# pipenv run python ./scripts/dashscope_poc.py -v "./data/video.mp4" -s "./prompts/system_prompt.txt" -m qwen3-vl-plus

import argparse
from pathlib import Path
import decouple
import dashscope


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("-s", "--system-prompt-path", type=str, required=True)
    parser.add_argument("-p", "--user-prompt-path", type=str, required=False)
    parser.add_argument("-v", "--video-path", type=str)
    parser.add_argument("-o", "--output-file", type=str, default="output.json")
    parser.add_argument("-f", "--fps", type=int, default=10)
    parser.add_argument("-m", "--model", type=str, default="qwen3-vl-plus")
    parser.add_argument("-t", "--thinking-budget", type=int, default=4000)
    return parser.parse_args()


def main() -> None:
    args = parse_args()

    dashscope.base_http_api_url = "https://dashscope-intl.aliyuncs.com/api/v1"
    api_key: str | None = decouple.config("DASHSCOPE_API_KEY", default=None)  # type: ignore
    assert api_key is not None, "DASHSCOPE_API_KEY environment variable is not set."
    video_path = Path(args.video_path)
    model = args.model
    fps = args.fps
    thinking_budget = args.thinking_budget
    output_file = Path(args.output_file)

    system_prompt = Path(args.system_prompt_path).read_text(encoding="utf-8")
    if args.user_prompt_path:
        user_prompt = Path(args.user_prompt_path).read_text(encoding="utf-8")
    else:
        user_prompt = "analyse this video"

    print(f"Analyzing video: {video_path}")
    print(f"Model: {model}, FPS: {fps}, Thinking budget: {thinking_budget}")
    print("-" * 60)

    messages = [
        {
            "role": "system",
            "content": system_prompt,
        },
        {
            "role": "user",
            "content": [
                {"video": f"file://{video_path}", "fps": fps},
                {"text": user_prompt},
            ],
        },
    ]

    response = dashscope.MultiModalConversation.call(
        api_key=api_key,
        model=model,
        messages=messages,
        stream=True,
        incremental_output=True,
        thinking_budget=thinking_budget,
        enable_thinking=True,
    )

    is_answering = False
    reasoning_content = ""
    answer_content = ""

    print("Streaming response...")

    for chunk in response:
        message = chunk.output.choices[0].message
        reasoning_content_chunk = message.get("reasoning_content", None)
        if message.content == [] and reasoning_content_chunk == "":
            continue

        if reasoning_content_chunk is not None:
            print(message.reasoning_content, end="")
            reasoning_content += message.reasoning_content
            continue

        if message.content != []:
            if not is_answering:
                print("\n" + "=" * 20 + "Complete Response" + "=" * 20)
                is_answering = True
            content = message.content[0]["text"]  # type: ignore
            print(content, end="")
            answer_content += content

    output_file.write_text(answer_content, encoding="utf-8")
    print(f"\n\nAnalysis saved to: {output_file.absolute()}")


if __name__ == "__main__":
    main()
